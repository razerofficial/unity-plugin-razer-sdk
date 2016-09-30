CALL gradlew clean build copyJar
COPY /Y java\libs\store-sdk-standard-release.aar ..
COPY /Y java\build\outputs\aar\java-release.aar ..\UnityPluginStoreSDK.aar
CALL copy_aar.cmd
