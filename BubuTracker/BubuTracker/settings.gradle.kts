pluginManagement {
    repositories {
        google {
            content {
                includeGroupByRegex("com\\.android.*")
                includeGroupByRegex("com\\.google.*")
                includeGroupByRegex("androidx.*")
            }
        }
        mavenCentral()
        gradlePluginPortal()
    }
}
dependencyResolutionManagement {
    repositoriesMode.set(RepositoriesMode.FAIL_ON_PROJECT_REPOS)
    repositories {
        google()
        mavenCentral()
    }
}

rootProject.name = "BubuTracker"
include(":app")

include(":core:common")
include(":core:designsystem")
include(":core:navigation")
include(":core:network")
include(":core:session")
include(":core:location")

include(":feature:auth")
include(":feature:map")
include(":feature:profile")
 