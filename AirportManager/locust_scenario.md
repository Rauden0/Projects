# PA165 Airline – User Profiles

This Locust test simulates role-based user behavior for the PA165 airline system.

For correct working the services must be running on localhost:
## User Roles

### JohnUser – Full System Validator
Simulates end-to-end integration: creates airports, airplanes, stewards, and flights, then verifies them.
The JohnUser is a test scenario he is a full system validator. He creates all entities in the correct order and verifies them. This user profile is used to test the entire system and its integration.
### CommercialDeptUser – Infrastructure Admin
Manages airports and airplanes only. No interaction with stewards or flights.
The CommercialDeptUser is a test scenario he is an infrastructure admin. He creates and manages airports and airplanes only. This user profile is used to test the infrastructure of the system.

### SchedulingUser – Flight Scheduler
Schedules flights between airports using available airplanes. Does not manage stewards.
The SchedulingUser is a test scenario he is a flight scheduler. He creates and manages flights between airports using available airplanes. This user profile is used to test the flight scheduling functionality of the system.

### HRUser – HR & Roster Manager
Manages stewards and assigns them to flights. Does not touch airports or airplanes.
The HRUser is a test scenario he is a HR and roster manager. He creates and manages stewards and assigns them to flights. This user profile is used to test the HR and roster management functionality of the system.

---

## Run Specific User Profile

Start Locust with the desired profile using the test profile dropdown or override the class:


```bash
locust -f locust_scenario.py --host http://localhost {profile}
```
The host is irrelevant for the Locust test, but it is required to run the test. The Locust test will use the host to send requests to the system. The host can be any URL, but it is recommended to use localhost for testing purposes.
With profile being one of the following:
- JohnUser
- CommercialDeptUser
- SchedulingUser
- HRUser
Depending on the profile, the Locust test will simulate different user behaviors and interactions with the system. Each profile has its own set of tasks and scenarios to validate the functionality of the PA165 airline system.

## Setup Locust
docker compose build
docker compose up -d
```bash
python3 -m venv venv
source venv/bin/activate
```

```bash
pip install locust
```

```bash
locust -f locust_scenario.py --host http://localhost {profile}
```
