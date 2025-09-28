package cz.muni.fi.pa165.spring2025.group1.team4.steward_service;

import io.swagger.v3.oas.annotations.OpenAPIDefinition;
import io.swagger.v3.oas.annotations.info.Info;
import io.swagger.v3.oas.annotations.servers.Server;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.data.web.config.EnableSpringDataWebSupport;
import org.springframework.data.web.config.EnableSpringDataWebSupport.PageSerializationMode;

@OpenAPIDefinition(info = @Info(title = "Steward Service", version = "0.6", description = """
		Create, retrieve, edit and delete flight stewards.
		    """), servers = @Server(url = "http://localhost:8079"))
@SpringBootApplication
@EnableSpringDataWebSupport(pageSerializationMode = PageSerializationMode.VIA_DTO)
public class StewardServiceApplication {

	public static void main(String[] args) {
		SpringApplication.run(StewardServiceApplication.class, args);
	}

}
