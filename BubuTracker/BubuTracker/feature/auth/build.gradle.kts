plugins {
    alias(libs.plugins.android.library)
    alias(libs.plugins.jetbrains.kotlin.android)
}

android {
    namespace = "com.example.bubutracker.feature.auth"
    compileSdk = 34

    defaultConfig {
        minSdk = 24
        testInstrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"

        buildConfigField("String", "AUTH0_DOMAIN", "\"YOUR_TENANT.auth0.com\"")
        buildConfigField("String", "AUTH0_CLIENT_ID", "\"YOUR_AUTH0_CLIENT_CLIENT_ID\"")
        buildConfigField("String", "AUTH0_AUDIENCE", "\"https://api.bubutracker\"")

        // Only needed so this module's OWN standalone androidTest APK can merge its
        // manifest; when built as part of :app, :app's own placeholders take over.
        manifestPlaceholders["auth0Domain"] = "YOUR_TENANT.auth0.com"
        manifestPlaceholders["auth0Scheme"] = "bubutracker"
    }

    buildFeatures {
        buildConfig = true
    }

    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_1_8
        targetCompatibility = JavaVersion.VERSION_1_8
    }
    kotlinOptions {
        jvmTarget = "1.8"
    }
}

dependencies {
    implementation(project(":core:network"))
    implementation(project(":core:session"))
    implementation(project(":core:navigation"))
    implementation(project(":core:designsystem"))

    implementation(libs.appcompat)
    implementation(libs.androidx.constraintlayout)
    implementation(libs.auth0)

    androidTestImplementation(libs.androidx.junit)
    androidTestImplementation(libs.androidx.espresso.core)
    androidTestImplementation(libs.androidx.espresso.intents)
    androidTestImplementation(libs.mockwebserver)
}
