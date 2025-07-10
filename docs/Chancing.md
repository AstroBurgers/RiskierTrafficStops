# Chancing System Overview

This document outlines how Riskier Traffic Stops evaluates a suspect's risk profile using weighted factors and
classifications. Each factor contributes points to one or more of the following categories:

- Violent
- Neutral
- Safe

These scores determine the risk classification and influence the outcome behavior during stops.

---

## Evaluation Criteria

### Driver’s License State

| License Status       | Risk Impact          |
|----------------------|----------------------|
| Expired / Unlicensed | +5 to Neutral        |
| Suspended            | +10 to Violent       |

---

### Times Stopped

For every time the suspect has been previously stopped:

- +1 to Safe
- +1 to Neutral
- +1 to Violent

---

### Wanted Status

If the suspect has an active warrant:

- +25 to Violent
- +10 to Neutral

---

### Vehicle BOLOs

If the suspect's vehicle has active BOLOs:

- Each BOLO adds:
    - +5 to Violent
    - +5 to Neutral

---

### Stolen Vehicle

If the vehicle is flagged as stolen:

- +25 to Violent

---

### Insurance Status

If the vehicle’s insurance is invalid (expired, revoked, or missing):

- +5 to Safe
- +5 to Neutral

---

### Registration Status

If the registration is invalid (expired, revoked, or missing):

- +5 to Safe
- +5 to Neutral
- +10 to Violent

---

### VIN Status

If the vehicle’s VIN is scratched or unreadable:

- +20 to Violent

---

## Outcome Classifications

Once the suspect’s scores are totaled, the risk classification is determined and used to pick a weighted outcome.

### Violent Outcomes

Selected for Violent classification:

- `GetOutAndShoot`
- `Ramming`
- `Flee`
- `ShootAndFlee`

---

### Neutral Outcomes

Selected for Neutral classification:

- `GetOutRO`
- `Yelling`
- `Revving`

---

### Safe Outcomes

Selected for Safe classification:

- `YellInCar`
- `Spitting`

---

## Notes

- The final classification is selected using a weighted random roll based on the total scores for each category.
- Only enabled outcomes are eligible to be selected.
- These values are subject to tuning and balancing in future updates.