package app.service;

import app.entity.Airplane;
import app.repository.AirplaneRepository;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;

@Service
public class AirplaneService {
    @Autowired
    private AirplaneRepository airplaneRepository;

    public Airplane getAirplaneById(Long id) {
        return airplaneRepository.findById(id).orElse(null);
    }

    @Transactional
    public Airplane createAirplane(Airplane airplane) {
        return airplaneRepository.save(airplane);
    }

    @Transactional
    public void deleteAirplane(Long id) {
        airplaneRepository.deleteById(id);
    }

    @Transactional
    public Airplane updateAirplane(Airplane airplane) {
        return airplaneRepository.save(airplane);
    }

    public List<Airplane> getAllAirplanes() {
        return airplaneRepository.findAll();
    }


}
