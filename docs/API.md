# RiskierTrafficStops API Reference

## Public Properties

### `DisableRTSForCurrentStop`

- **Type**: `bool` (get/set)
- **Description**: Disables RTS from interacting with the *current or next* pullover.
- **Note**: Automatically resets after each stop.

---

## Public Methods

### `DisableRTSForPeds(Ped[] peds)`

- **Description**: Disables RTS behavior for the specified peds.
- **Logic**:
    - Invalid peds are cleared every 10 in-game minutes.
    - Valid peds persist across cleanup cycles.
    - If the driver is in this list, the outcome is ended immediately.

### `GetPedRisk(Ped suspect)`

- **Description**: Calculates the overall risk score of a suspect ped based on their profile and vehicle data.
- **Parameters**:
    - `suspect` — The ped suspect to evaluate risk for.
- **Returns**:
    - An integer representing the combined risk score. Returns 0 if the suspect does not exist or data is invalid.
- **Remarks**:
    - Requires that the suspect is in a vehicle, as the risk profile is also based on the suspect’s last vehicle data.
    - Utilizes data pulled from [CommonDataFramework](https://github.com/Policing-Redefined/CommonDataFramework/tree/main)

### `GetPedRiskSummary(Ped suspect)`

- **Description**: Calculates a detailed risk summary for a suspect ped, breaking down the violent, neutral, and safe
  risk scores separately.
- **Parameters**:
    - `suspect` — The ped suspect to evaluate risk for.
- **Returns**:
    - A `PedRiskSummary` struct containing individual risk scores. Returns default (all zeros) if the suspect does not
      exist or data is invalid.
- **Remarks**:
    - Requires that the suspect is in a vehicle, as the risk profile is also based on the suspect’s last vehicle data.
    - Utilizes data pulled from [CommonDataFramework](https://github.com/Policing-Redefined/CommonDataFramework/tree/main)

---

## Structs

### `PedRiskSummary`

- **Description**: Represents a detailed breakdown of risk scores for a suspect ped, including violent, neutral, and
  safe risk components.
- **Properties**:
    - `ViolentScore` (int) — The violent risk component.
    - `NeutralScore` (int) — The neutral risk component.
    - `SafeScore` (int) — The safe risk component.
    - `TotalScore` (int) — The total combined risk score, used internally when choosing outcomes.

---

## Events

### `OnRTSOutcomeStarted`

- Triggered when a RTS outcome is triggered.

### `OnRTSOutcomeEnded`

- Triggered when an RTS outcome finishes.

---

### Missing Something?

Open an issue or ping me on Discord if any part of the API is unclear.