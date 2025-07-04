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

---

## Events

### `OnRTSOutcomeStarted`
- Triggered when a RTS outcome is triggered.

### `OnRTSOutcomeEnded`
- Triggered when an RTS outcome finishes.

---

### Missing Something?

Open an issue or ping me on Discord if any part of the API is unclear.
