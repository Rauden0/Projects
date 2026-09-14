import java.util.Properties

plugins {
    alias(libs.plugins.android.application)
    alias(libs.plugins.jetbrains.kotlin.android)
}

val secrets = rootProject.extra["bubutrackerSecrets"] as Properties

fun secret(key: String, default: String): String =
    secrets.getProperty(key)?.trim()?.takeIf { it.isNotEmpty() } ?: default

fun isPlaceholder(value: String): Boolean {
    val markers = listOf(
        "YOUR_",
        "example.com",
        "YOUR_TENANT",
        "YOUR_AUTH0",
        "YOUR_STORE",
        "YOUR_KEY",
        "REPLACE",
        "dev-REPLACE",
    )
    return value.isBlank() ||
        value.equals("secret", ignoreCase = true) ||
        markers.any { value.contains(it, ignoreCase = true) }
}

val auth0Domain = secret("auth0Domain", "YOUR_TENANT.auth0.com")
val auth0Scheme = secret("auth0Scheme", "bubutracker")
val apiBaseUrlDebug = secret("apiBaseUrlDebug", "http://10.0.2.2:8080/")
val apiBaseUrlRelease = secret("apiBaseUrlRelease", "https://api.bubutracker.example.com/")

val keystorePropertiesFile = rootProject.file("keystore.properties")
val keystoreProperties = Properties().apply {
    if (keystorePropertiesFile.exists()) {
        keystorePropertiesFile.inputStream().use { load(it) }
    }
}

android {
    namespace = "com.example.bubutracker"
    compileSdk = 36

    defaultConfig {
        // Play Store identity — permanent after first upload.
        applicationId = "com.martinmucka.bubutracker"
        minSdk = 24
        targetSdk = 36
        versionCode = 1
        versionName = "1.0.0"

        testInstrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"

        manifestPlaceholders["auth0Domain"] = auth0Domain
        manifestPlaceholders["auth0Scheme"] = auth0Scheme
    }

    signingConfigs {
        create("release") {
            if (keystorePropertiesFile.exists()) {
                val storePath = keystoreProperties.getProperty("storeFile")
                    ?: error("keystore.properties missing storeFile")
                storeFile = rootProject.file(storePath)
                storePassword = keystoreProperties.getProperty("storePassword")
                    ?: error("keystore.properties missing storePassword")
                keyAlias = keystoreProperties.getProperty("keyAlias")
                    ?: error("keystore.properties missing keyAlias")
                keyPassword = keystoreProperties.getProperty("keyPassword")
                    ?: error("keystore.properties missing keyPassword")
            }
        }
    }

    buildTypes {
        debug {
            // Emulator localhost alias; port matches BubuTrackerAPI-Go default.
            buildConfigField("String", "API_BASE_URL", "\"$apiBaseUrlDebug\"")
        }
        release {
            isMinifyEnabled = false
            proguardFiles(
                getDefaultProguardFile("proguard-android-optimize.txt"),
                "proguard-rules.pro"
            )
            buildConfigField("String", "API_BASE_URL", "\"$apiBaseUrlRelease\"")

            if (keystorePropertiesFile.exists()) {
                signingConfig = signingConfigs.getByName("release")
            }
        }
    }
    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_1_8
        targetCompatibility = JavaVersion.VERSION_1_8
    }
    kotlinOptions {
        jvmTarget = "1.8"
    }
    buildFeatures {
        buildConfig = true
    }
    packaging {
        resources {
            excludes += "/META-INF/{AL2.0,LGPL2.1}"
        }
    }
}

tasks.register("verifyReleaseSecrets") {
    group = "verification"
    description = "Ensures secrets.properties has real production values for release builds."
    doLast {
        val required = mapOf(
            "auth0Domain" to auth0Domain,
            "auth0ClientId" to secret("auth0ClientId", "YOUR_AUTH0_CLIENT_ID"),
            "auth0Audience" to secret("auth0Audience", "https://api.bubutracker"),
            "apiBaseUrlRelease" to apiBaseUrlRelease,
            "privacyPolicyUrl" to secret("privacyPolicyUrl", "https://YOUR_DOMAIN/bubutracker/privacy"),
        )
        val bad = required.filter { (_, v) -> isPlaceholder(v) || v.equals("secret", ignoreCase = true) }
        if (bad.isNotEmpty()) {
            throw GradleException(
                "Release secrets still look like placeholders: ${bad.keys.joinToString()}. " +
                    "Copy secrets.properties.example → secrets.properties and set real values."
            )
        }
        if (!keystorePropertiesFile.exists()) {
            throw GradleException(
                "Missing keystore.properties. Copy keystore.properties.example and generate upload-keystore.jks."
            )
        }
        val storeFile = rootProject.file(
            keystoreProperties.getProperty("storeFile")
                ?: error("keystore.properties missing storeFile")
        )
        if (!storeFile.exists()) {
            throw GradleException("Upload keystore not found at ${storeFile.absolutePath}")
        }
    }
}

tasks.matching { it.name.matches(Regex("bundleRelease|assembleRelease|packageReleaseBundle")) }.configureEach {
    dependsOn("verifyReleaseSecrets")
}

dependencies {
    implementation(project(":core:designsystem"))
    implementation(project(":core:navigation"))
    implementation(project(":core:network"))
    implementation(project(":core:session"))
    implementation(project(":core:location"))

    implementation(project(":feature:auth"))
    implementation(project(":feature:map"))
    implementation(project(":feature:profile"))

    implementation(libs.androidx.core.ktx)
    implementation(libs.androidx.lifecycle.process)

    testImplementation(libs.junit)
    androidTestImplementation(libs.androidx.junit)
    androidTestImplementation(libs.androidx.espresso.core)
    androidTestImplementation(libs.mockwebserver)
}
