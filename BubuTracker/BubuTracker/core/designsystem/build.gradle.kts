import java.util.Properties

plugins {
    alias(libs.plugins.android.library)
    alias(libs.plugins.jetbrains.kotlin.android)
}

val secrets = rootProject.extra["bubutrackerSecrets"] as Properties

fun secret(key: String, default: String): String =
    secrets.getProperty(key)?.trim()?.takeIf { it.isNotEmpty() } ?: default

val privacyPolicyUrl = secret(
    "privacyPolicyUrl",
    "https://YOUR_DOMAIN/bubutracker/privacy"
)

android {
    namespace = "com.example.bubutracker.core.designsystem"
    compileSdk = 36

    defaultConfig {
        minSdk = 24
        resValue("string", "privacy_policy_url", privacyPolicyUrl)
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
    // Theme.BubuTracker extends Theme.MaterialComponents — needed on compile classpath.
    api(libs.material)
}
