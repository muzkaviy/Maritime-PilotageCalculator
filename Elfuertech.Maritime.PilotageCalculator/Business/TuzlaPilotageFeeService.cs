using Elfuertech.Maritime.PilotageCalculator.Core;

namespace Elfuertech.Maritime.PilotageCalculator.Business;

public class TuzlaPilotageFeeService
{
    private const int InitialPriceGrtIntervalBegin = 1;
    private const int InitialPriceGrtIntervalEnd = 499;
    private const int BasePriceGrtIntervalBegin = 500;
    private const int BasePriceGrtIntervalEnd = 999;
    private const int GrossTonLimitForPricing = 120000;
    private static readonly List<TuzlaGisasPricePackage> PricePackageList = GetPricePackageList();
    private static readonly TuzlaGisasPricePackage InitialPricePackage = GetInitialPricePackage();
    private static readonly TuzlaGisasPricePackage BasePricePackage = GetBasePricePackage();
    private static readonly TuzlaGisasPricePackage IncrementPricePackage = GetIncrementPricePackage();
    private static readonly Dictionary<string, List<TugCount>> TugCountDictionary = GetTugCountDictionary();
    
    public PilotageCalculationResponse Calculate(TuzlaPilotageFeeCalculateCommand command)
    {
        var pilotageCalculationResult = CalculatePilotage(command);

        command.OnSiteShifting = true;
        command.IsMaintenance = true;

        var respShiftingWithMachine = CalculatePilotage(command);

        var shiftingWithMachineNetPrice =
            (respShiftingWithMachine.TugPricing?.NetPrice ?? 0) +
            (respShiftingWithMachine.MooringPricing?.NetPrice ?? 0) +
            (respShiftingWithMachine.PilotagePricing?.NetPrice ?? 0) +
            (respShiftingWithMachine.AdditionalTugPricing?.NetPrice ?? 0);

        pilotageCalculationResult.ShiftingWithMainEnginePowerBerthToFromDdPricing = new PilotagePricing(shiftingWithMachineNetPrice, 0, command.ManeuverCancellation);

        command.IsThereMachineFailureAtVessel = true;
        
        var respShiftingWithoutMachine = CalculatePilotage(command);

        var shiftingWithoutMachineNetPrice =
            (respShiftingWithoutMachine.TugPricing?.NetPrice ?? 0) +
            (respShiftingWithoutMachine.MooringPricing?.NetPrice ?? 0) +
            (respShiftingWithoutMachine.PilotagePricing?.NetPrice ?? 0) +
            (respShiftingWithoutMachine.AdditionalTugPricing?.NetPrice ?? 0);

        pilotageCalculationResult.ShiftingWithoutMainEnginePowerBerthToFromDdPricing = new PilotagePricing(shiftingWithoutMachineNetPrice, 0, command.ManeuverCancellation);
        
        var pilotageCalculationResponse = new PilotageCalculationResponse
        {
            InOutPricing = new PilotageCalculationPricing(pilotageCalculationResult.InOutPricing.NetPrice, pilotageCalculationResult.InOutPricing.CurrencyType),
            PilotagePricing = new PilotageCalculationPricing(pilotageCalculationResult.PilotagePricing.NetPrice, pilotageCalculationResult.PilotagePricing.CurrencyType),
            MooringPricing = new PilotageCalculationPricing(pilotageCalculationResult.MooringPricing.NetPrice, pilotageCalculationResult.MooringPricing.CurrencyType),
            TugPricing = new PilotageCalculationPricing(pilotageCalculationResult.TugPricing.NetPrice, pilotageCalculationResult.TugPricing.CurrencyType),
            ShiftingWithMainEnginePowerBerthToFromDdPricing = new PilotageCalculationPricing(pilotageCalculationResult.ShiftingWithMainEnginePowerBerthToFromDdPricing.NetPrice, pilotageCalculationResult.ShiftingWithMainEnginePowerBerthToFromDdPricing.CurrencyType),
            ShiftingWithoutMainEnginePowerBerthToFromDdPricing = new PilotageCalculationPricing(pilotageCalculationResult.ShiftingWithoutMainEnginePowerBerthToFromDdPricing.NetPrice, pilotageCalculationResult.ShiftingWithoutMainEnginePowerBerthToFromDdPricing.CurrencyType)
        };        

        return pilotageCalculationResponse;
    }
    
    private static PilotageCalculationDto CalculatePilotage(TuzlaPilotageFeeCalculateCommand command)
    {
        var tugCount = GetTugCount(command);

        var pilotageCalculationResponse = new PilotageCalculationDto
        {
            InOutCount = new PilotageCount(0, 0),
            PilotageCount = new PilotageCount(1, 1),
            MooringCount = new PilotageCount(1, 1),
            TugCount = new PilotageCount(tugCount, tugCount),
            InOutPricing = new PilotagePricing(command.ManeuverCancellation),
            PilotagePricing = new PilotagePricing(command.ManeuverCancellation),
            MooringPricing = new PilotagePricing(command.ManeuverCancellation),
            TugPricing = new PilotagePricing(command.ManeuverCancellation)
        };

        if (string.IsNullOrWhiteSpace(command.ShippingType))
        {
            return pilotageCalculationResponse;
        }

        var (pilotageDiscountOrHike, tugDiscountOrHike, mooringDiscountOrHike) = GetDiscountOrHikes(command);

        var pricePackage = GetConvenientPricePackage(command);

        var pilotagePrice = pricePackage.PriceDictionary[command.ShippingType].Pilotage * pilotageCalculationResponse.PilotageCount.TotalCount;
        var mooringPrice = pricePackage.PriceDictionary[command.ShippingType].Mooring * pilotageCalculationResponse.MooringCount.TotalCount;
        var tugPrice = pricePackage.PriceDictionary[command.ShippingType].Tug * pilotageCalculationResponse.TugCount.TotalCount;

        if (command.OnSiteShifting)
        {
            pilotagePrice *= 0.5m;
            mooringPrice *= 0.5m;
            tugPrice *= 0.5m;
        }

        pilotageCalculationResponse.InOutPricing = new PilotagePricing(command.ManeuverCancellation);
        pilotageCalculationResponse.PilotagePricing = new PilotagePricing((int)pilotagePrice, pilotageDiscountOrHike, command.ManeuverCancellation);
        pilotageCalculationResponse.MooringPricing = new PilotagePricing((int)mooringPrice, mooringDiscountOrHike, command.ManeuverCancellation);
        pilotageCalculationResponse.TugPricing = new PilotagePricing((int)tugPrice, tugDiscountOrHike, command.ManeuverCancellation);

        return pilotageCalculationResponse;
    }

    private static TuzlaGisasPricePackage GetConvenientPricePackage(TuzlaPilotageFeeCalculateCommand command)
    {
        switch (command.GrossTon)
        {
            case <= InitialPriceGrtIntervalEnd:
                return InitialPricePackage;

            case <= BasePriceGrtIntervalEnd:
                return BasePricePackage;

            default:
            {
                var factor = Math.Min(command.GrossTon, GrossTonLimitForPricing) / 1000;
                var pricePackage = new TuzlaGisasPricePackage(command.GrossTon, command.GrossTon);

                foreach (var shippingType in TuzlaGisasPilotageShippingTypes.All)
                {
                    var pilotage = BasePricePackage.PriceDictionary[shippingType].Pilotage + factor * IncrementPricePackage.PriceDictionary[shippingType].Pilotage;
                    var tug = BasePricePackage.PriceDictionary[shippingType].Tug + factor * IncrementPricePackage.PriceDictionary[shippingType].Tug;
                    var mooring = BasePricePackage.PriceDictionary[shippingType].Mooring + factor * IncrementPricePackage.PriceDictionary[shippingType].Mooring;
                    pricePackage.PriceDictionary[shippingType] = new TuzlaGisasPrice(pilotage, tug, mooring);
                }

                pricePackage.Constants = new TuzlaGisasConstant(
                    skil: BasePricePackage.Constants.Skil + factor * IncrementPricePackage.Constants.Skil,
                    demir: BasePricePackage.Constants.Demir + factor * IncrementPricePackage.Constants.Demir,
                    rs: factor < 2 ? 0 : factor < 5 ? 1 : factor < 30 ? 2 : 3,
                    IncrementPricePackage.Constants.Cer,
                    IncrementPricePackage.Constants.Ref,
                    IncrementPricePackage.Constants.Pilot,
                    IncrementPricePackage.Constants.Rombek);

                return pricePackage;
            }
        }
    }

    private static int GetTugCount(TuzlaPilotageFeeCalculateCommand command)
    {
        var tugCount = 0;
        var tugDictionaryIndex = command.IsVesselCarryingHazardousMaterial && command.ShippingType == TuzlaGisasPilotageShippingTypes.Other ? "Hazardous" : "Other";
        var rsGroup = TugCountDictionary[tugDictionaryIndex];

        foreach (var tc in rsGroup)
        {
            if (tc.GrtIntervalEnd < command.GrossTon)
            {
                continue;
            }

            tugCount = tc.Count;

            if (command.IsThereMachineFailureAtVessel && command.GrossTon < 5001)
            {
                tugCount = 2;
            }

            break;
        }

        return tugCount;
    }

    private static List<TuzlaGisasPricePackage> GetPricePackageList()
    {
        var list = new List<TuzlaGisasPricePackage>();

        var pricePackage1 = new TuzlaGisasPricePackage(InitialPriceGrtIntervalBegin, InitialPriceGrtIntervalEnd);
        pricePackage1.PriceDictionary.Add(TuzlaGisasPilotageShippingTypes.CabotageLine, new TuzlaGisasPrice(70, 119, 11));
        pricePackage1.PriceDictionary.Add(TuzlaGisasPilotageShippingTypes.RoRo, new TuzlaGisasPrice(116, 224, 22));
        pricePackage1.PriceDictionary.Add(TuzlaGisasPilotageShippingTypes.Container, new TuzlaGisasPrice(153, 299, 22));
        pricePackage1.PriceDictionary.Add(TuzlaGisasPilotageShippingTypes.Other, new TuzlaGisasPrice(197, 373, 22));
        pricePackage1.Constants = new TuzlaGisasConstant(78, 20, 0, 600, 400, 120, 180);
        list.Add(pricePackage1);

        var pricePackage2 = new TuzlaGisasPricePackage(BasePriceGrtIntervalBegin, BasePriceGrtIntervalEnd);
        pricePackage2.PriceDictionary.Add(TuzlaGisasPilotageShippingTypes.CabotageLine, new TuzlaGisasPrice(70, 119, 11));
        pricePackage2.PriceDictionary.Add(TuzlaGisasPilotageShippingTypes.RoRo, new TuzlaGisasPrice(116, 224, 22));
        pricePackage2.PriceDictionary.Add(TuzlaGisasPilotageShippingTypes.Container, new TuzlaGisasPrice(153, 299, 22));
        pricePackage2.PriceDictionary.Add(TuzlaGisasPilotageShippingTypes.Other, new TuzlaGisasPrice(197, 373, 22));
        pricePackage2.Constants = new TuzlaGisasConstant(155, 20, 0, 600, 400, 120, 180);
        list.Add(pricePackage2);

        var pricePackage3 = new TuzlaGisasPricePackage(BasePriceGrtIntervalEnd + 1, int.MaxValue);
        pricePackage3.PriceDictionary.Add(TuzlaGisasPilotageShippingTypes.CabotageLine, new TuzlaGisasPrice(25, 25, 6));
        pricePackage3.PriceDictionary.Add(TuzlaGisasPilotageShippingTypes.RoRo, new TuzlaGisasPrice(46, 40, 11));
        pricePackage3.PriceDictionary.Add(TuzlaGisasPilotageShippingTypes.Container, new TuzlaGisasPrice(65, 56, 11));
        pricePackage3.PriceDictionary.Add(TuzlaGisasPilotageShippingTypes.Other, new TuzlaGisasPrice(81, 70, 11));
        pricePackage3.Constants = new TuzlaGisasConstant(65, 18, 0, 600, 400, 120, 180);
        list.Add(pricePackage3);

        return list;
    }

    private static TuzlaGisasPricePackage GetInitialPricePackage()
    {
        return PricePackageList
            .FirstOrDefault(x => x.GrtIntervalBegin == InitialPriceGrtIntervalBegin);
    }

    private static TuzlaGisasPricePackage GetBasePricePackage()
    {
        return PricePackageList
            .FirstOrDefault(x => x.GrtIntervalBegin == BasePriceGrtIntervalBegin);
    }

    private static TuzlaGisasPricePackage GetIncrementPricePackage()
    {
        return PricePackageList
            .FirstOrDefault(x => x.GrtIntervalBegin == BasePriceGrtIntervalEnd + 1);
    }

    private static Dictionary<string, List<TugCount>> GetTugCountDictionary()
    {
        var dict = new Dictionary<string, List<TugCount>>
        {
            ["Other"] =
            [
                new TugCount(1, 5000, 1),
                new TugCount(5001, int.MaxValue, 2)
            ],
            ["Hazardous"] =
            [
                new TugCount(1, 5000, 1),
                new TugCount(5001, 45000, 1),
                new TugCount(45001, int.MaxValue, 1)
            ]
        };

        return dict;
    }

    private static (int, int, int) GetDiscountOrHikes(TuzlaPilotageFeeCalculateCommand command)
    {
        var pilotageDiscountOrHikeList = new List<int>();
        var tugDiscountOrHikeList = new List<int>();
        var mooringDiscountOrHikeList = new List<int>();

        if (command.IsVesselCarryingHazardousMaterial && !command.IsGasFree)
        {
            switch (command.ShippingType)
            {
                case TuzlaGisasPilotageShippingTypes.RoRo:
                case TuzlaGisasPilotageShippingTypes.Container:
                    pilotageDiscountOrHikeList.Add(20);
                    tugDiscountOrHikeList.Add(20);
                    mooringDiscountOrHikeList.Add(20);
                    break;

                case TuzlaGisasPilotageShippingTypes.CabotageLine:
                case TuzlaGisasPilotageShippingTypes.Other:
                    pilotageDiscountOrHikeList.Add(30);
                    tugDiscountOrHikeList.Add(30);
                    mooringDiscountOrHikeList.Add(30);
                    break;
            }
        }

        if (command.IsThereMachineFailureAtVessel)
        {
            tugDiscountOrHikeList.Add(50);
        }

        if (command.IsPublicHoliday)
        {
            pilotageDiscountOrHikeList.Add(50);
            tugDiscountOrHikeList.Add(50);
            mooringDiscountOrHikeList.Add(50);
        }

        if (command.IsMaintenance)
        {
            pilotageDiscountOrHikeList.Add(-40);
            tugDiscountOrHikeList.Add(-40);
            mooringDiscountOrHikeList.Add(-40);
        }

        if (command.IsVesselTurkishFlagged)
        {
            pilotageDiscountOrHikeList.Add(-20);
            tugDiscountOrHikeList.Add(-20);
            mooringDiscountOrHikeList.Add(-20);
        }

        var pilotageHike = pilotageDiscountOrHikeList.Any(x => x > 0) ? pilotageDiscountOrHikeList.Max() : 0;
        var pilotageDiscount = pilotageDiscountOrHikeList.Any(x => x < 0) ? pilotageDiscountOrHikeList.Min() : 0;
        var pilotageDiscountOrHike = pilotageHike + pilotageDiscount;

        var tugHike = tugDiscountOrHikeList.Any(x => x > 0) ? tugDiscountOrHikeList.Max() : 0;
        var tugDiscount = tugDiscountOrHikeList.Any(x => x < 0) ? tugDiscountOrHikeList.Min() : 0;
        var tugDiscountOrHike = tugHike + tugDiscount;

        var mooringHike = mooringDiscountOrHikeList.Any(x => x > 0) ? mooringDiscountOrHikeList.Max() : 0;
        var mooringDiscount = mooringDiscountOrHikeList.Any(x => x < 0) ? mooringDiscountOrHikeList.Min() : 0;
        var mooringDiscountOrHike = mooringHike + mooringDiscount;

        return (pilotageDiscountOrHike, tugDiscountOrHike, mooringDiscountOrHike);
    }
}