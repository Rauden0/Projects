plugins {
    alias(libs.plugins.android.application)
    alias(libs.plugins.jetbrains.kotlin.android)
}

android {
    namespace = "com.example.bubutracker"
    compileSdk = 34

    defaultConfig {
        applicationId = "com.example.bubutracker"
        minSdk = 24
        targetSdk = 34
        versionCode = 1
        versionName = "1.0"

        testInstrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
        vectorDrawables {
            useSupportLibrary = true
        }

        buildConfigField("String", "API_BASE_URL", "\"http://10.0.2.2:5121/\"")
        buildConfigField("String", "AUTH0_DOMAIN", "\"YOUR_TENANT.auth0.com\"")
        buildConfigField("String", "AUTH0_CLIENT_ID", "\"YOUR_AUTH0_CLIENT_CLIENT_ID\"")
        buildConfigField("String", "AUTH0_AUDIENCE", "\"https://api.bubutracker\"")
        manifestPlaceholders["auth0Domain"] = "YOUR_TENANT.auth0.com"
        manifestPlaceholders["auth0Scheme"] = "bubutracker"
    }

    buildTypes {
        release {
            isMinifyEnabled = false
            proguardFiles(
                getDefaultProguardFile("proguard-android-optimize.txt"),
                "proguard-rules.pro"
            )
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
        compose = true
        buildConfig = true
    }
    composeOptions {
        kotlinCompilerExtensionVersion = "1.5.1"
    }
    packaging {
        resources {
            excludes += "/META-INF/{AL2.0,LGPL2.1}"
        }
    }
}

dependencies {
    implementation(libs.androidx.core.ktx)
    implementation(libs.androidx.lifecycle.runtime.ktx)
    implementation(libs.androidx.activity.compose)
    implementation(platform(libs.androidx.compose.bom))
    implementation(libs.androidx.ui)
    implementation(libs.androidx.ui.graphics)
    implementation(libs.androidx.ui.tooling.preview)
    implementation(libs.androidx.material3)
    implementation(libs.appcompat)
    implementation(libs.androidx.appcompat.v141)
    implementation(libs.material.v160)
    implementation(libs.androidx.constraintlayout)
    implementation(libs.play.services.maps.v1900)
    implementation(libs.play.services.location)
    implementation(libs.retrofit)
    implementation(libs.converter.gson)
    implementation(libs.gson)
    implementation(libs.okhttp)
    implementation(libs.okhttp.logging)
    implementation(libs.auth0)
    implementation(libs.kotlinx.coroutines.core)

    testImplementation(libs.junit)
    testImplementation("org.robolectric:robolectric:4.11.1")
    androidTestImplementation(libs.androidx.junit)
    androidTestImplementation(libs.androidx.espresso.core)
    androidTestImplementation(platform(libs.androidx.compose.bom))
    androidTestImplementation(libs.androidx.ui.test.junit4)
    debugImplementation(libs.androidx.ui.tooling)
    debugImplementation(libs.androidx.ui.test.manifest)
}
