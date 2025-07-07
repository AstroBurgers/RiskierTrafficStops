# Architecture Overview

This document provides a high-level look at how RTS is structured and why.

---

## 📁 Key Namespaces

### `RiskierTrafficStops.Engine`
Core systems that control plugin behavior, game integration, and logic dispatch.

### `RiskierTrafficStops.Mod.Outcomes`
Each outcome is a modular class representing a specific suspect behavior. These are randomized and triggered based on context.

### `RiskierTrafficStops.API`
Exposes safe hooks for other developers to interact with or override RTS logic.

---

## 🔁 Event Flow

1. LSPD:FR initiates a pullover
2. RTS checks if the conditions for an event are met
3. RTS Randomizes a value and compares that to the users set chance
4. Outcome runs until terminated (passively or via user action)
5. Cleanup logic and reset flags run post-stop

---

## 🛠️ Key Systems

- `GameFiberHandling.cs`: Ensures safe threading/timing
- `PulloverEventHandler.cs`: Hooks into the core LSPD:FR pullover lifecycle
- `Settings/IniReflector.cs`: Parses `.ini` config settings into runtime flags
- `Helpers/*.cs`: Math, pursuit, and general utility helpers
