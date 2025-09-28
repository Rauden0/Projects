package app.config;

import cz.muni.fi.pa165.spring2025.group1.team4.security.SecurityScopes;
import io.swagger.v3.oas.models.security.SecurityScheme;
import org.springdoc.core.customizers.OpenApiCustomizer;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.http.HttpMethod;
import org.springframework.security.config.Customizer;
import org.springframework.security.config.annotation.web.builders.HttpSecurity;
import org.springframework.security.web.SecurityFilterChain;

@Configuration
public class SecurityConfiguration {
    public static final String SCHEME = "Bearer";

    private static final String COMMERCIAL_DEPT = "SCOPE_" + SecurityScopes.COMMERCIAL_DEPT;
    private static final String SCHEDULE_COORDINATOR = "SCOPE_" + SecurityScopes.SCHEDULE_COORDINATOR;
    private static final String HR_AND_MANAGEMENT = "SCOPE_" + SecurityScopes.HR_AND_MANAGEMENT;

    private static final SecurityScheme SECURITY_SCHEME_BEARER = new SecurityScheme()
            .type(SecurityScheme.Type.HTTP)
            .name(SCHEME)
            .scheme("bearer")
            .description("provide a valid OAuth2 token from http://localhost:8090/swagger-ui.html");

    @Bean
    SecurityFilterChain securityFilterChain(HttpSecurity http) throws Exception {
        http
                .authorizeHttpRequests(x -> x
                        // Read airport and airplane info
                        .requestMatchers(HttpMethod.GET, "/flights", "/flights/**")
                        .hasAnyAuthority(COMMERCIAL_DEPT, SCHEDULE_COORDINATOR, HR_AND_MANAGEMENT)
                        // Manage airports and airplanes
                        .requestMatchers("/flights", "/flights/**")
                        .hasAnyAuthority(SCHEDULE_COORDINATOR)
                        .anyRequest().permitAll())
                .oauth2ResourceServer(oauth2 -> oauth2.opaqueToken(Customizer.withDefaults()));
        return http.build();
    }

    @Bean
    OpenApiCustomizer openApiCustomizer() {
        return openApi -> {
            openApi
                    .getComponents()
                    .addSecuritySchemes(SECURITY_SCHEME_BEARER.getName(), SECURITY_SCHEME_BEARER);
        };
    }

}
