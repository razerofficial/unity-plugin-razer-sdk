CALL gradlew clean build copyJar
COPY /Y java\libs\store-sdk-standard-release.aar ..\Assets\Plugins\Android\libs
PAUSE
