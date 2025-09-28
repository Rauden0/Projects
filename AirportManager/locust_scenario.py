import json
from datetime import datetime, timedelta
from time import sleep

import requests
from locust import HttpUser, task, between, events
import random

# BASE URLS
AIRPORT_SERVICE = "http://localhost:8081"
STEWARD_SERVICE = "http://localhost:8079"
FLIGHT_SERVICE = "http://localhost:8080"

# OAUTH2 CONFIG
TOKEN_ENDPOINT = "https://id.muni.cz/oidc/token"
CLIENT_ID = "d57b3a8f-156e-46de-9f27-39c4daee05e1"
CLIENT_SECRET = "fa228ebc-4d54-4cda-901e-4d6287f8b1652a9c9c44-73c9-4502-973f-bcdb4a8ec96a"
SCOPES = "test_1 test_2 test_3"


def get_access_token(scopes=SCOPES):
    data = {
        "grant_type": "client_credentials",
        "scope": scopes,
    }
    response = requests.post(
        TOKEN_ENDPOINT,
        auth=(CLIENT_ID, CLIENT_SECRET),
        data=data
    )
    response.raise_for_status()
    return response.json()["access_token"]


def tracked_request(method, url, token=None, **kwargs):
    from requests.exceptions import RequestException
    if method == "delete":
        print(f"Deleting resource... {url}")
    headers = kwargs.pop("headers", {})
    if token:
        headers["Authorization"] = f"Bearer {token}"
    kwargs["headers"] = headers
    print(kwargs)
    request_meta = {
        "request_type": method.upper(),
        "name": url.replace("http://localhost:", ""),
        "start_time": datetime.now(),
        "response_length": 0,
        "response": None,
        "exception": None,
        "context": {},
    }

    try:
        response = getattr(requests, method.lower())(url, **kwargs)
        request_meta["response"] = response
        request_meta["response_length"] = len(response.content)
        request_meta["status_code"] = response.status_code

        total_time = (datetime.now() - request_meta["start_time"]).total_seconds() * 1000
        events.request.fire(
            request_type=request_meta["request_type"],
            name=request_meta["name"],
            response_time=total_time,
            response_length=request_meta["response_length"],
            response=response,
            exception=None if response.ok else f"HTTP {response.status_code}",
            context={}
        )
        if response.content:
            print(f"Method: {method.upper()}, URL: {url}, Status: {response.status_code}, Time: {total_time:.2f}ms")
            print(f"Request: {request_meta['name']}, Status: {response.status_code}, Time: {total_time:.2f}ms")
            print(json.dumps(response.json(), indent=4))
        return response
    except RequestException as e:
        total_time = (datetime.now() - request_meta["start_time"]).total_seconds() * 1000
        events.request.fire(
            request_type=request_meta["request_type"],
            name=request_meta["name"],
            response_time=total_time,
            response_length=0,
            exception=str(e),
            context={}
        )


class BaseFlightUser(HttpUser):
    wait_time = between(1, 2)
    abstract = True

    def on_start(self):
        self.token = get_access_token()
        self.airports = []
        self.airplanes = []
        self.stewards = []
        self.flights = []
        self.create_airport("Brno")
        self.create_airport("Vienna")
        self.create_airport("Prague")
        self.create_airplane(self.airports[0])
        self.create_airplane(self.airports[1])
        self.create_airplane(self.airports[2])
        self.create_flight(self.airports[0], self.airports[1], self.airplanes[0],
                           dep_time=(datetime.utcnow() + timedelta(hours=3)).isoformat(),
                           arr_time=(datetime.utcnow() + timedelta(hours=6)).isoformat())

    def create_airport(self, city=None):
        data = {
            "name": f"Airport {random.randint(1000, 9999)}",
            "city": city or random.choice(["New York", "London", "Paris", "Tokyo", "Brno", "Vienna", "Prague"]),
            "country": random.choice(["USA", "UK", "France", "Japan", "Czech Republic"]),
            "capacity": random.randint(1000, 5000),
        }
        response = tracked_request("post", f"{AIRPORT_SERVICE}/airports", json=data, token=self.token)
        if response and response.status_code == 201:
            self.airports.append(response.json()["id"])

    def create_airplane(self, airport_id):
        data = {
            "name": f"Airplane {random.randint(1000, 9999)}",
            "capacity": random.randint(100, 300),
            "type": random.choice(["PRIVATE", "BUSINESS_JET","CARGO"]),
            "airportId": airport_id,
            "maximumTravelDistance": random.randint(1000, 10000),
        }
        response = tracked_request("post", f"{AIRPORT_SERVICE}/airplanes", json=data, token=self.token)
        if response and response.status_code == 201:
            self.airplanes.append(response.json()["id"])

    def create_steward(self):
        data = {
            "givenName": random.choice(["Alice", "Bob", "Charlie", "Dana"]),
            "familyName": random.choice(["Smith", "Johnson", "Williams", "Brown"])
        }
        response = tracked_request("post", f"{STEWARD_SERVICE}/stewards", json=data, token=self.token)
        if response and response.status_code == 201:
            self.stewards.append(response.json()["id"])

    def create_flight(self, dep_id, arr_id, plane_id, dep_time, arr_time):
        data = {
            "flightCode": f"FLIGHT{random.randint(1000, 9999)}",
            "departureAirportId": dep_id,
            "arrivalAirportId": arr_id,
            "planeId": plane_id,
            "totalSeats": random.randint(100, 300),
            "price": random.randint(100, 1000),
            "departureTime": dep_time,
            "arrivalTime": arr_time,
            "availableSeats": random.randint(100, 200),
            "status": "ACTIVE"
        }
        response = tracked_request("post", f"{FLIGHT_SERVICE}/flights", json=data, token=self.token)
        if response and response.status_code == 201:
            self.flights.append(response.json()["id"])

    def view_random_flight(self):
        if self.flights:
            flight_id = random.choice(self.flights)
            tracked_request("get", f"{FLIGHT_SERVICE}/flights/{flight_id}", token=self.token)

    def get_airport(self, airport_id):
        tracked_request("get", f"{AIRPORT_SERVICE}/airports/{airport_id}", token=self.token)

    def get_airplane(self, airplane_id):
        tracked_request("get", f"{AIRPORT_SERVICE}/airplanes/{airplane_id}", token=self.token)

    def delete_airport(self, airport_id):
        tracked_request("delete", f"{AIRPORT_SERVICE}/airports/{airport_id}", token=self.token)

    def delete_airplane(self, airplane_id):
        tracked_request("delete", f"{AIRPORT_SERVICE}/airplanes/{airplane_id}", token=self.token)

    def update_airport(self, data):
        tracked_request("put", f"{AIRPORT_SERVICE}/airports", json=data, token=self.token)

    def update_airplane(self, data):
        tracked_request("put", f"{AIRPORT_SERVICE}/airplanes", json=data, token=self.token)

    def delete_flight(self, flight_id):
        tracked_request("delete", f"{FLIGHT_SERVICE}/flights/{flight_id}", token=self.token)

    def update_flight(self, data):
        tracked_request("put", f"{FLIGHT_SERVICE}/flights", json=data, token=self.token)


class JohnUser(BaseFlightUser):
    def on_start(self):
        super().on_start()
        self.create_airport("Tokyo")
        self.create_airport("London")
        self.create_airplane(self.airports[0])
        while len(self.stewards) < 2:
            self.create_steward()
        self.create_flight(
            self.airports[0], self.airports[1], int(self.airplanes[1]),
            (datetime.utcnow() + timedelta(hours=3)).isoformat(),
            (datetime.utcnow() + timedelta(hours=6)).isoformat()
        )
        tracked_request("get", f"{AIRPORT_SERVICE}/airports", token=self.token)
        tracked_request("get", f"{AIRPORT_SERVICE}/airplanes", token=self.token)
        tracked_request("put", f"{AIRPORT_SERVICE}/airports", json={
            "id":self.airports[0], "name": "Updated Tokyo Airport", "city": "Tokyo"
        }, token=self.token)
        for sid in self.stewards:
            tracked_request("get", f"{STEWARD_SERVICE}/stewards/{sid}", token=self.token)
        self.view_random_flight()
        self.environment.runner.quit()

class CommercialDeptUser(BaseFlightUser):
    def on_start(self):
        super().on_start()
        self.token = get_access_token(scopes="test_1")
        self.create_airport("Prague")
        self.create_airport("Berlin")
        self.create_airplane(self.airports[0])
        self.create_airplane(self.airports[1])
        tracked_request("get", f"{AIRPORT_SERVICE}/airports", token=self.token)
        tracked_request("get", f"{AIRPORT_SERVICE}/airplanes", token=self.token)
        self.update_airport({
             "id":self.airports[0] ,"name": "Updated Prague Airport", "city": "Prague"
        })
        self.update_airplane({
            "id":self.airplanes[0],"name": "Updated Airplane", "capacity": 250
        })
        self.delete_airport(self.airports[1])
        self.delete_airplane(self.airplanes[1])
        self.environment.runner.quit()

class SchedulingUser(BaseFlightUser):
    def on_start(self):
        super().on_start()
        self.token = get_access_token(scopes="test_2")
        self.create_flight(
            self.airports[0], self.airports[1], self.airplanes[2], (datetime.utcnow() + timedelta(hours=2)).isoformat(),
            (datetime.utcnow() + timedelta(hours=5)).isoformat()
        )
        self.view_random_flight()
        self.update_flight(
            {
                "id": self.flights[0],
                "availableSeats": 150,
                "status": "INACTIVE"}
        )
        self.delete_flight(self.flights[0])
        sleep(1)
        self.environment.runner.quit()

class HRUser(BaseFlightUser):
    def on_start(self):
        super().on_start()
        self.token = get_access_token(scopes="test_3")
        sleep(1)
        for _ in range(3):
            self.create_steward()
        for sid in self.stewards:
            tracked_request("get", f"{STEWARD_SERVICE}/stewards/{sid}", token=self.token)
        if self.flights and len(self.stewards) >= 2:
            tracked_request("put", f"{STEWARD_SERVICE}/flights/{self.flights[0]}/stewards/{int(self.stewards[0])}", token=self.token)
            tracked_request("put", f"{STEWARD_SERVICE}/flights/{self.flights[0]}/stewards/{int(self.stewards[1])}", token=self.token)

        tracked_request("patch", f"{STEWARD_SERVICE}/stewards", json={
            "id":int(self.stewards[0]), "familyName": "UpdatedFirst", "givenName": "UpdatedLast"
        }, token=self.token)
        self.environment.runner.quit()

available_profiles = {
    "John": JohnUser,
    "Admin": CommercialDeptUser,
    "Staff": SchedulingUser,
    "HR": HRUser,
}


def get_user_classes_from_test_profile(profile_name: str):
    return [available_profiles.get(profile_name, JohnUser)]
