using Elfuertech.Maritime.PilotageCalculator.Core;

namespace Elfuertech.Maritime.PilotageCalculator.Business;

public class YalovaPilotageFeeService
{
    private static readonly Dictionary<string, List<List<int>>> PriceDictionary = GetPriceDictionary();

    public PilotageCalculationResponse Calculate(YalovaPilotageFeeCalculateCommand command)
    {
        var validationResponse = ValidateCommand(command);

        if (!validationResponse.Succeeded)
        {
            return validationResponse;
        }
        
        var pilotageCalculationResult = CalculatePilotage(command);

        command.OnSiteShifting = true;
        command.IsMaintenance = true;
        
        var shiftingWithMachineResult = CalculatePilotage(command);

        var shiftingWithMachineNetPrice =
            (shiftingWithMachineResult.TugPricing?.NetPrice ?? 0) +
            (shiftingWithMachineResult.MooringPricing?.NetPrice ?? 0) +
            (shiftingWithMachineResult.PilotagePricing?.NetPrice ?? 0) +
            (shiftingWithMachineResult.AdditionalTugPricing?.NetPrice ?? 0);

        pilotageCalculationResult.ShiftingWithMainEnginePowerBerthToFromDdPricing = new PilotagePricing(shiftingWithMachineNetPrice, 0);

        command.IsThereMachineFailureAtVessel = true;
        
        var shiftingWithoutMachineResult = CalculatePilotage(command);

        var shiftingWithoutMachineNetPrice =
            (shiftingWithoutMachineResult.TugPricing?.NetPrice ?? 0) +
            (shiftingWithoutMachineResult.MooringPricing?.NetPrice ?? 0) +
            (shiftingWithoutMachineResult.PilotagePricing?.NetPrice ?? 0) +
            (shiftingWithoutMachineResult.AdditionalTugPricing?.NetPrice ?? 0);

        pilotageCalculationResult.ShiftingWithoutMainEnginePowerBerthToFromDdPricing = new PilotagePricing(shiftingWithoutMachineNetPrice, 0);

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
    
    private static PilotageCalculationResponse ValidateCommand(YalovaPilotageFeeCalculateCommand command)
    {
        var response = new PilotageCalculationResponse
        {
            Succeeded = true
        };

        if (!YalovaPilotageShippingTypes.All.Contains(command.ShippingType))
        {
            var allYalovaVesselTypes = string.Join(", ", YalovaPilotageShippingTypes.All);

            response.Succeeded = false;
            response.ErrorCode = "WrongVesselType";
            response.ErrorDetail = $"The vessel type must be one of them: {allYalovaVesselTypes}";
        }
        else if (command.GrossTon <= 0)
        {
            response.Succeeded = false;
            response.ErrorCode = "WrongGrossTon";
            response.ErrorDetail = "The gross ton must be bigger than 0";
        }

        return response;
    }

    private static PilotageCalculationDto CalculatePilotage(YalovaPilotageFeeCalculateCommand command)
    {
        var response = new PilotageCalculationDto
        {
            InOutCount = new PilotageCount(0, 0),
            PilotageCount = new PilotageCount(0, 0),
            MooringCount = new PilotageCount(0, 0),
            TugCount = new PilotageCount(0, 0),
            InOutPricing = new PilotagePricing(),
            PilotagePricing = new PilotagePricing(),
            MooringPricing = new PilotagePricing(),
            TugPricing = new PilotagePricing()
        };

        int mooringDiscount,
            mooringHike,
            tugDiscount,
            tugHike,
            inOutDiscount,
            inOutHike,
            pilotageDiscount,
            pilotageHike;

        List<int> item;

        switch (command.ShippingType)
        {
            case YalovaPilotageShippingTypes.RoRo:
                item = PriceDictionary[YalovaPilotageShippingTypes.RoRo]
                    .FirstOrDefault(x =>
                        command.GrossTon >= x[0] &&
                        command.GrossTon <= x[1]);

                if (item is null)
                {
                    break;
                }

                mooringDiscount = 0;
                tugDiscount = 0;
                inOutDiscount = 0;
                pilotageDiscount = 0;
                mooringHike = 0;
                tugHike = 0;
                inOutHike = 0;
                pilotageHike = 0;

                if (command.IsVesselCarryingHazardousMaterial)
                {
                    tugHike = 20;
                    inOutHike = 20;
                    mooringHike = 20;
                    pilotageHike = 20;
                }

                if (command.IsThereMachineFailureAtVessel)
                {
                    tugHike = 50;
                }

                if (command.IsPublicHoliday)
                {
                    tugHike = 50;
                    inOutHike = 50;
                    mooringHike = 50;
                    pilotageHike = 50;
                }

                if (command.IsVesselTurkishFlagged)
                {
                    tugDiscount = 20;
                    inOutDiscount = 20;
                    mooringDiscount = 20;
                    pilotageDiscount = 20;
                }

                if (command.IsMaintenance)
                {
                    tugDiscount = 40;
                    inOutDiscount = 40;
                    mooringDiscount = 40;
                    pilotageDiscount = 40;
                }

                if (command.OnSiteShifting)
                {
                    tugDiscount = 40;
                    inOutDiscount = 40;
                    pilotageDiscount = 40;
                    mooringDiscount = 40;

                    if (command.IsMaintenance)
                    {
                        mooringHike = 0;

                        if (command.IsThereMachineFailureAtVessel)
                        {
                            tugHike = 10;
                            tugDiscount = 0;
                        }
                    }

                    if (command.IsPublicHoliday)
                    {
                        mooringHike = 10;
                        mooringDiscount = 0;
                    }
                }

                response.InOutCount = new PilotageCount(1, 1);
                response.InOutPricing = new PilotagePricing(item[5] * 2, inOutHike - inOutDiscount);

                response.MooringCount = new PilotageCount(1, 1);
                response.MooringPricing = new PilotagePricing(item[3] * 2, mooringHike - mooringDiscount);

                if (command.GrossTon > 5000 || 
                    command.IsThereMachineFailureAtVessel)
                {
                    response.TugPricing = new PilotagePricing(item[4] * 4, tugHike - tugDiscount);
                    response.TugCount = new PilotageCount(2, 2);
                }
                else
                {
                    response.TugPricing = new PilotagePricing(item[4] * 2, tugHike - tugDiscount);
                    response.TugCount = new PilotageCount(1, 1);
                }

                response.PilotageCount = new PilotageCount(1, 1);
                response.PilotagePricing = new PilotagePricing(item[6] * 2, pilotageHike - pilotageDiscount);

                if (command.OnSiteShifting)
                {
                    command.NumberOfAdditionalTugsAtTheExit = 0;
                }

                var additionalTugPrice = item[4] * (command.NumberOfAdditionalTugsAtTheEntrance + command.NumberOfAdditionalTugsAtTheExit);
                response.AdditionalTugPricing = new PilotagePricing(additionalTugPrice, 0);

                if (command.OnSiteShifting)
                {
                    response.InOutPricing = new PilotagePricing(0, 0);
                    response.MooringPricing.Price /= 2;
                    response.TugPricing.Price /= 2;
                    response.PilotagePricing.Price /= 2;
                }

                break;

            case YalovaPilotageShippingTypes.CabotageLine:
                item = PriceDictionary[YalovaPilotageShippingTypes.CabotageLine]
                    .FirstOrDefault(x => 
                        command.GrossTon >= x[0] && 
                        command.GrossTon <= x[1]);

                if (item is null)
                {
                    break;
                }

                mooringDiscount = 0;
                tugDiscount = 0;
                inOutDiscount = 0;
                pilotageDiscount = 0;
                mooringHike = 0;
                tugHike = 0;
                inOutHike = 0;
                pilotageHike = 0;

                if (command.IsVesselCarryingHazardousMaterial)
                {
                    tugHike = 30;
                    inOutHike = 30;
                    mooringHike = 30;
                    pilotageHike = 30;
                }

                if (command.IsThereMachineFailureAtVessel)
                {
                    tugHike = 50;
                    inOutDiscount = 50;
                }

                if (command.IsPublicHoliday)
                {
                    tugHike = 50;
                    inOutHike = 50;
                    mooringHike = 50;
                    pilotageHike = 50;
                }

                if (command.IsMaintenance)
                {
                    tugDiscount = 40;
                    inOutDiscount = 40;
                    mooringDiscount = 40;
                    pilotageDiscount = 40;
                }

                if (command.IsVesselTurkishFlagged)
                {
                    inOutDiscount = 50;
                }

                if (command.OnSiteShifting)
                {
                    tugDiscount = 40;
                    inOutDiscount = 40;
                    pilotageDiscount = 40;
                    mooringDiscount = 40;

                    if (command.IsMaintenance)
                    {
                        mooringHike = 0;

                        if (command.IsThereMachineFailureAtVessel)
                        {
                            tugHike = 10;
                            tugDiscount = 0;
                        }
                    }

                    if (command.IsPublicHoliday)
                    {
                        mooringHike = 10;
                        mooringDiscount = 0;
                    }
                }

                if (command.IsVesselCarryingHazardousMaterial)
                {
                    if (inOutDiscount < 20)
                    {
                        inOutDiscount = 20;
                    }
                }

                if (!command.IsVesselTurkishFlagged &&
                    !command.IsThereMachineFailureAtVessel &&
                    !command.IsMaintenance &&
                    !command.IsPublicHoliday &&
                    !command.IsVesselCarryingHazardousMaterial &&
                    !command.OnSiteShifting)
                {
                    inOutDiscount = 50;
                }

                if (command.IsMaintenance &&
                    command.IsThereMachineFailureAtVessel &&
                    command.IsPublicHoliday)
                {
                    tugDiscount = 0;
                    tugHike = 10;
                }

                if (command.IsVesselCarryingHazardousMaterial)
                {
                    inOutHike = 0;
                    inOutDiscount = 20;
                }

                if (command.IsPublicHoliday)
                {
                    inOutHike = 0;
                    inOutDiscount = 0;
                }

                response.InOutCount = new PilotageCount(1, 1);
                response.InOutPricing = new PilotagePricing(item[5] * 2, inOutHike - inOutDiscount);

                response.MooringCount = new PilotageCount(1, 1);
                response.MooringPricing = new PilotagePricing(item[3] * 2, mooringHike - mooringDiscount);

                if (command.GrossTon > 5000 || 
                    command.IsThereMachineFailureAtVessel)
                {
                    response.TugPricing = new PilotagePricing(item[4] * 4, tugHike - tugDiscount);
                    response.TugCount = new PilotageCount(2, 2);
                }
                else
                {
                    response.TugPricing = new PilotagePricing(item[4] * 2, tugHike - tugDiscount);
                    response.TugCount = new PilotageCount(1, 1);
                }
                
                response.PilotageCount = new PilotageCount(1, 1);
                response.PilotagePricing = new PilotagePricing(item[6] * 2, pilotageHike - pilotageDiscount);

                if (command.OnSiteShifting)
                {
                    command.NumberOfAdditionalTugsAtTheExit = 0;
                }

                additionalTugPrice = item[4] * (command.NumberOfAdditionalTugsAtTheEntrance + command.NumberOfAdditionalTugsAtTheExit);
                response.AdditionalTugPricing = new PilotagePricing(additionalTugPrice, 0);

                if (command.OnSiteShifting)
                {
                    response.InOutPricing = new PilotagePricing(0, 0);
                    response.MooringPricing.Price /= 2;
                    response.TugPricing.Price /= 2;
                    response.PilotagePricing.Price /= 2;
                    response.AdditionalTugPricing.Price /= 2;
                }

                break;

            case YalovaPilotageShippingTypes.Other:
                item = PriceDictionary[YalovaPilotageShippingTypes.Other]
                    .FirstOrDefault(x => 
                        command.GrossTon >= x[0] &&
                        command.GrossTon <= x[1]);

                if (item is null)
                {
                    break;
                }

                mooringDiscount = 0;
                tugDiscount = 0;
                inOutDiscount = 0;
                pilotageDiscount = 0;
                mooringHike = 0;
                tugHike = 0;
                inOutHike = 0;
                pilotageHike = 0;

                if (command.IsVesselCarryingHazardousMaterial)
                {
                    tugHike = 30;
                    inOutHike = 30;
                    mooringHike = 30;
                    pilotageHike = 30;
                }

                if (command.IsThereMachineFailureAtVessel)
                {
                    tugHike = 50;
                }

                if (command.IsPublicHoliday)
                {
                    tugHike = 50;
                    inOutHike = 50;
                    mooringHike = 50;
                    pilotageHike = 50;
                }

                if (command.IsMaintenance || command.IsVesselTurkishFlagged)
                {
                    tugDiscount = 40;
                    inOutDiscount = 40;
                    mooringDiscount = 40;
                    pilotageDiscount = 40;
                }

                if (command.OnSiteShifting)
                {
                    tugDiscount = 40;
                    inOutDiscount = 40;
                    pilotageDiscount = 40;
                    mooringDiscount = 40;

                    if (command.IsMaintenance)
                    {
                        mooringHike = 0;

                        if (command.IsThereMachineFailureAtVessel)
                        {
                            tugHike = 10;
                            tugDiscount = 0;
                        }
                    }
                }

                if (command.IsMaintenance &&
                    command.IsThereMachineFailureAtVessel &&
                    command.IsPublicHoliday)
                {
                    tugDiscount = 0;
                    tugHike = 10;
                }

                response.InOutCount = new PilotageCount(1, 1);
                response.InOutPricing = new PilotagePricing(item[5] * 2, inOutHike - inOutDiscount);

                response.MooringCount = new PilotageCount(1, 1);
                response.MooringPricing = new PilotagePricing(item[3] * 2, mooringHike - mooringDiscount);

                if (command.GrossTon > 5000 || 
                    command.IsThereMachineFailureAtVessel)
                {
                    response.TugPricing = new PilotagePricing(item[4] * 4, tugHike - tugDiscount);
                    response.TugCount = new PilotageCount(2, 2);
                }
                else
                {
                    response.TugPricing = new PilotagePricing(item[4] * 2, tugHike - tugDiscount);
                    response.TugCount = new PilotageCount(1, 1);
                }

                
                response.PilotageCount = new PilotageCount(1, 1);
                response.PilotagePricing = new PilotagePricing(item[6] * 2, pilotageHike - pilotageDiscount);

                if (command.OnSiteShifting)
                {
                    command.NumberOfAdditionalTugsAtTheExit = 0;
                }

                additionalTugPrice = item[4] * (command.NumberOfAdditionalTugsAtTheEntrance + command.NumberOfAdditionalTugsAtTheExit);
                response.AdditionalTugPricing = new PilotagePricing(additionalTugPrice, 0);

                if (command.OnSiteShifting)
                {
                    response.InOutPricing = new PilotagePricing(0, 0);
                    response.MooringPricing.Price /= 2;
                    response.TugPricing.Price /= 2;
                    response.PilotagePricing.Price /= 2;
                }

                break;

            case YalovaPilotageShippingTypes.Container:
                item = PriceDictionary[YalovaPilotageShippingTypes.Container]
                    .FirstOrDefault(x => 
                        command.GrossTon >= x[0] &&
                        command.GrossTon <= x[1]);

                if (item is null)
                {
                    break;
                }

                mooringDiscount = 0;
                tugDiscount = 0;
                inOutDiscount = 0;
                pilotageDiscount = 0;
                mooringHike = 0;
                tugHike = 0;
                inOutHike = 0;
                pilotageHike = 0;

                if (command.IsVesselCarryingHazardousMaterial)
                {
                    tugHike = 30;
                    inOutHike = 30;
                    mooringHike = 30;
                    pilotageHike = 30;
                }

                if (command.IsThereMachineFailureAtVessel)
                {
                    tugHike = 50;
                }

                if (command.IsPublicHoliday)
                {
                    tugHike = 50;
                    inOutHike = 50;
                    mooringHike = 50;
                    pilotageHike = 50;
                }

                if (command.IsMaintenance || command.IsVesselTurkishFlagged)
                {
                    tugDiscount = 40;
                    inOutDiscount = 40;
                    mooringDiscount = 40;
                    pilotageDiscount = 40;
                }

                if (command.OnSiteShifting)
                {
                    tugDiscount = 40;
                    inOutDiscount = 40;
                    pilotageDiscount = 40;
                    mooringDiscount = 40;

                    if (command.IsMaintenance)
                    {
                        mooringHike = 0;

                        if (command.IsThereMachineFailureAtVessel)
                        {
                            tugHike = 10;
                            tugDiscount = 0;
                        }
                    }
                }

                if (command.IsMaintenance &&
                    command.IsThereMachineFailureAtVessel &&
                    command.IsPublicHoliday)
                {
                    tugDiscount = 0;
                    tugHike = 10;
                }

                response.InOutCount = new PilotageCount(1, 1);
                response.InOutPricing = new PilotagePricing(item[5] * 2, inOutHike - inOutDiscount);

                response.MooringCount = new PilotageCount(1, 1);
                response.MooringPricing = new PilotagePricing(item[3] * 2, mooringHike - mooringDiscount);

                if (command.GrossTon > 5000 || 
                    command.IsThereMachineFailureAtVessel)
                {
                    response.TugPricing = new PilotagePricing(item[4] * 4, tugHike - tugDiscount);
                    response.TugCount = new PilotageCount(2, 2);
                }
                else
                {
                    response.TugPricing = new PilotagePricing(item[4] * 2, tugHike - tugDiscount);
                    response.TugCount = new PilotageCount(1, 1);
                }

                
                response.PilotageCount = new PilotageCount(1, 1);
                response.PilotagePricing = new PilotagePricing(item[6] * 2, pilotageHike - pilotageDiscount);

                if (command.OnSiteShifting)
                {
                    command.NumberOfAdditionalTugsAtTheExit = 0;
                }

                additionalTugPrice = item[4] * (command.NumberOfAdditionalTugsAtTheEntrance + command.NumberOfAdditionalTugsAtTheExit);
                response.AdditionalTugPricing = new PilotagePricing(additionalTugPrice, 0);

                if (command.OnSiteShifting)
                {
                    response.InOutPricing = new PilotagePricing(0, 0);
                    response.MooringPricing.Price /= 2;
                    response.TugPricing.Price /= 2;
                    response.PilotagePricing.Price /= 2;
                }

                break;
        }

        return response;
    }

    private static Dictionary<string, List<List<int>>> GetPriceDictionary()
    {
        var dict = new Dictionary<string, List<List<int>>>();

        //////////
        // roro //
        var roroPriceList = new List<List<int>>();

        var price = 0;
        var mooring = 0;
        var tug = 0;
        var inOut = 0;
        var pilotage = 0;
        var i = 0;

        while (i < 120000)
        {
            int j;

            switch (i)
            {
                case 0:
                    j = i;
                    i = 1000;
                    price = 152;
                    mooring = 22;
                    tug = 224;
                    inOut = 152;
                    pilotage = 116;
                    break;

                case < 1001:
                    j = i;
                    i = 1001;
                    price = 152;
                    mooring = 22;
                    tug = 224;
                    inOut = 152;
                    pilotage = 116;
                    break;

                default:
                    j = i;
                    i += 1000;
                    price += 73;
                    mooring += 11;
                    tug += 40;
                    inOut += 73;
                    pilotage += 46;
                    break;
            }

            roroPriceList.Add([j, i, price, mooring, tug, inOut, pilotage]);
        }

        roroPriceList.Add([i, int.MaxValue, 8839, 1331, 4984, 8839, 5590]);
        dict.Add(YalovaPilotageShippingTypes.RoRo, roroPriceList);

        /////////////
        // kabotaj //
        var cabotageLinePriceList = new List<List<int>>();

        price = 0;
        mooring = 0;
        tug = 0;
        inOut = 0;
        pilotage = 0;
        i = 0;

        while (i < 120000)
        {
            int j;

            switch (i)
            {
                case 0:
                    j = i;
                    i = 1000;
                    price = 152;
                    mooring = 11;
                    tug = 119;
                    inOut = 152;
                    pilotage = 70;
                    break;

                case < 1001:
                    j = i;
                    i = 1001;
                    price = 152;
                    mooring = 11;
                    tug = 119;
                    inOut = 152;
                    pilotage = 70;
                    break;

                default:
                    j = i;
                    i += 1000;
                    price += 73;
                    mooring += 6;
                    tug += 25;
                    inOut += 73;
                    pilotage += 25;
                    break;
            }

            cabotageLinePriceList.Add([j, i, price, mooring, tug, inOut, pilotage]);
        }

        cabotageLinePriceList.Add([i, int.MaxValue, 8839, 725, 3094, 8839, 3045]);
        dict.Add(YalovaPilotageShippingTypes.CabotageLine, cabotageLinePriceList);

        
        ////////////////////
        // yabancı bayrak //
        var otherPriceList = new List<List<int>>();

        price = 0;
        mooring = 0;
        tug = 0;
        inOut = 0;
        pilotage = 0;
        i = 0;

        while (i < 120000)
        {
            int j;

            switch (i)
            {
                case 0:
                    j = i;
                    i = 1000;
                    price = 152;
                    mooring = 22;
                    tug = 373;
                    inOut = 152;
                    pilotage = 197;
                    break;

                case < 1001:
                    j = i;
                    i = 1001;
                    price = 152;
                    mooring = 22;
                    tug = 373;
                    inOut = 152;
                    pilotage = 197;
                    break;

                default:
                    j = i;
                    i += 1000;
                    price += 73;
                    mooring += 11;
                    tug += 70;
                    inOut += 73;
                    pilotage += 81;
                    break;
            }

            otherPriceList.Add([j, i, price, mooring, tug, inOut, pilotage]);
        }

        otherPriceList.Add([i, int.MaxValue, 8839, 1331, 8703, 8839, 9836]);
        dict.Add(YalovaPilotageShippingTypes.Other, otherPriceList);

        var containerPriceList = new List<List<int>>();

        price = 0;
        mooring = 0;
        tug = 0;
        inOut = 0;
        pilotage = 0;
        i = 0;

        while (i < 120000)
        {
            int j;

            switch (i)
            {
                case 0:
                    j = i;
                    i = 1000;
                    price = 152;
                    mooring = 22;
                    tug = 299;
                    inOut = 152;
                    pilotage = 153;
                    break;

                case < 1001:
                    j = i;
                    i = 1001;
                    price = 152;
                    mooring = 22;
                    tug = 299;
                    inOut = 152;
                    pilotage = 153;
                    break;

                default:
                    j = i;
                    i += 1000;
                    price += 73;
                    mooring += 11;
                    tug += 56;
                    inOut += 73;
                    pilotage += 65;
                    break;
            }

            containerPriceList.Add([j, i, price, mooring, tug, inOut, pilotage]);
        }

        containerPriceList.Add([i, int.MaxValue, 8839, 1331, 6963, 8839, 7888]);
        dict.Add(YalovaPilotageShippingTypes.Container, containerPriceList);

        return dict;
    }
}