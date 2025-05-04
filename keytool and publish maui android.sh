/Users/ozgur/jdk-11.0.27+6/Contents/Home/bin/keytool -genkeypair -v -keystore ozgur-elfuertech-pilotage-calculator.keystore -alias ozgur-elfuertech-pilotage-calculator -keyalg RSA -keysize 2048 -validity 10000

/Users/ozgur/jdk-11.0.27+6/Contents/Home/bin/keytool -list -keystore ozgur-elfuertech-pilotage-calculator.keystore

dotnet publish -f net8.0-android -c Release -p:AndroidKeyStore=true -p:AndroidSigningKeyStore=ozgur-elfuertech-pilotage-calculator.keystore -p:AndroidSigningKeyAlias=ozgur-elfuertech-pilotage-calculator -p:AndroidSigningKeyPass=123456 -p:AndroidSigningStorePass=123456

dotnet publish -f:net8.0-android -c Release -p:AndroidPackageFormat=aab
