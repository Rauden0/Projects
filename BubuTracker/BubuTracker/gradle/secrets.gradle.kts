import java.util.Properties

val secrets = Properties()
val secretsFile = rootProject.file("secrets.properties")
if (secretsFile.exists()) {
    secretsFile.inputStream().use { secrets.load(it) }
}
rootProject.extra["bubutrackerSecrets"] = secrets
