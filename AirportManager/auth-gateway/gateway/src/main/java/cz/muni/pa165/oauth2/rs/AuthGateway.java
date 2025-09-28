package cz.muni.pa165.oauth2.rs;

import io.swagger.v3.oas.annotations.OpenAPIDefinition;
import io.swagger.v3.oas.annotations.info.Info;
import io.swagger.v3.oas.annotations.servers.Server;
import io.swagger.v3.oas.models.security.OAuthFlow;
import io.swagger.v3.oas.models.security.OAuthFlows;
import io.swagger.v3.oas.models.security.Scopes;
import io.swagger.v3.oas.models.security.SecurityScheme;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springdoc.core.customizers.OpenApiCustomizer;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.boot.web.servlet.context.ServletWebServerInitializedEvent;
import org.springframework.context.annotation.Bean;
import org.springframework.context.event.EventListener;
import org.springframework.security.config.Customizer;
import org.springframework.security.config.annotation.web.builders.HttpSecurity;
import org.springframework.security.web.SecurityFilterChain;

import static cz.muni.fi.pa165.spring2025.group1.team4.security.SecurityScopes.*;

@SpringBootApplication
@OpenAPIDefinition(
        info = @Info(title = "Airport Manager OAuth2 Gateway", version = "1.0"),
        servers = @Server(description = "gateway server", url = "http://localhost:8090")
)
public class AuthGateway {

    private static final Logger log = LoggerFactory.getLogger(AuthGateway.class);
    private static final String SECURITY_SCHEME_OAUTH2 = "MUNI";
    private static final String SECURITY_SCHEME_BEARER = "Bearer";
    public static final String SECURITY_SCHEME_NAME = SECURITY_SCHEME_OAUTH2;

    public static void main(String[] args) {
        SpringApplication.run(AuthGateway.class, args);
    }

    /**
     * Configure access restrictions to the API.
     * Introspection of opaque access token is configured, introspection endpoint is defined in application.yml.
     */
    @Bean
    SecurityFilterChain securityFilterChain(HttpSecurity http) throws Exception {
        http
                .authorizeHttpRequests(x -> x
                        .anyRequest().permitAll()
                )
                .oauth2ResourceServer(oauth2 -> oauth2.opaqueToken(Customizer.withDefaults()))
        ;
        return http.build();
    }

    /**
     * Add security definitions to generated openapi.yaml.
     */
    @Bean
    public OpenApiCustomizer openAPICustomizer() {
        return openApi -> {
            log.info("adding security to OpenAPI description");
            openApi.getComponents()

                    .addSecuritySchemes(SECURITY_SCHEME_OAUTH2,
                            new SecurityScheme()
                                    .type(SecurityScheme.Type.OAUTH2)
                                    .description("get access token with OAuth 2 Authorization Code Grant")
                                    .flows(new OAuthFlows()
                                            .authorizationCode(new OAuthFlow()
                                                    .authorizationUrl("https://id.muni.cz/oidc/authorize")
                                                    .tokenUrl("https://id.muni.cz/oidc/token")
                                                    .scopes(new Scopes()
                                                            .addString(COMMERCIAL_DEPT, "Commercial Department: Manage airplanes and airports")
                                                            .addString(SCHEDULE_COORDINATOR, "Schedule Coordinator: Manage flights and flight staffing")
                                                            .addString(HR_AND_MANAGEMENT, "HR and Management: Manage stewards and steward shifts")
                                                    )
                                            )
                                    )
                    )
                    /*.addSecuritySchemes(SECURITY_SCHEME_BEARER,
                            new SecurityScheme()
                                    .type(SecurityScheme.Type.HTTP)
                                    .scheme("bearer")
                                    .description("provide a valid access token")
                    )*/
            ;
        };
    }

    /**
     * Display a hint in the log.
     */
    @EventListener
    public void onApplicationEvent(ServletWebServerInitializedEvent event) {
        log.info("**************************");
        int port = event.getWebServer().getPort();
        log.info("visit http://localhost:{}/swagger-ui.html for UI", port);
        log.info("visit http://localhost:{}/openapi.yaml for OpenAPI document", port);
        log.info("**************************");
    }

}
