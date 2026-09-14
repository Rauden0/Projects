import java.util.Properties

plugins {
    alias(libs.plugins.android.library)
    alias(libs.plugins.jetbrains.kotlin.android)
}

val secrets = rootProject.extra["bubutrackerSecrets"] as Properties

fun secret(key: String, default: String): String =
    secrets.getProperty(key)?.trim()?.takeIf { it.isNotEmpty() } ?: default

val auth0Domain = secret("auth0Domain", "YOUR_TENANT.auth0.com")
val auth0ClientId = secret("auth0ClientId", "YOUR_AUTH0_CLIENT_ID")
val auth0Audience = secret("auth0Audience", "https://api.bubutracker")
val auth0Scheme = secret("auth0Scheme", "bubutracker")

android {
    namespace = "com.example.bubutracker.feature.auth"
    compileSdk = 36

    defaultConfig {
        minSdk = 24
        testInstrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"

        buildConfigField("String", "AUTH0_DOMAIN", "\"$auth0Domain\"")
        buildConfigField("String", "AUTH0_CLIENT_ID", "\"$auth0ClientId\"")
        buildConfigField("String", "AUTH0_AUDIENCE", "\"$auth0Audience\"")

        // Placeholders for this module's standalone androidTest APK merge.
        manifestPlaceholders["auth0Domain"] = auth0Domain
        manifestPlaceholders["auth0Scheme"] = auth0Scheme
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
