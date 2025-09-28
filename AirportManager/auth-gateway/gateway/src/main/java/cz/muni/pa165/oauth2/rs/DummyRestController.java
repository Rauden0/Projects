package cz.muni.pa165.oauth2.rs;

import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.security.SecurityRequirement;
import io.swagger.v3.oas.annotations.tags.Tag;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
@RequestMapping(path = "/api")
@Tag(name = "Dummy API")
public class DummyRestController {

    private static final String SECURITY_SCHEME_OAUTH2 = "MUNI";

    @Operation(summary = "Pick up your token here", security = @SecurityRequirement(name = SECURITY_SCHEME_OAUTH2))
    @PostMapping(path = "/dummy")
    public void doNothing() {
    }
}
