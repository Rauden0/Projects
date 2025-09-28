package app.entity;

import jakarta.persistence.Entity;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import lombok.*;

@Entity
@Table(name = "airplanes")
@Getter
@Setter
@NoArgsConstructor
@AllArgsConstructor
@Builder
public class Airplane {
    @Id
    private Long id;
}