# RiskierTrafficStops [![wakatime](https://wakatime.com/badge/github/AstroBurgers/RiskierTrafficStops.svg)](https://wakatime.com/badge/github/AstroBurgers/RiskierTrafficStops) ![LOC](https://img.shields.io/badge/Lines%20of%20Code-2.9k+-blue)


A LSPDFR plugin that adds randomized risk and tension to traffic stops by injecting unique suspect behaviors—ranging from verbal confrontation to outright violence.

## 🔧 Features

- Randomized traffic stop outcomes to keep encounters unpredictable
- Configurable via `.ini` and fully localizable via `.json`
- Developer-friendly API and public event hooks
- Supports sub-outcomes and mod extensibility

---

## Current Outcomes

- **Get Out and Shoot** – The suspect(s) exit and open fire on you and other officers.
- **Flee** – The suspect(s) flee either in their vehicle or on foot.
- **Yell** – The suspect exits the vehicle and verbally confronts you (3 sub-outcomes).
- **Ramming** – The suspect reverses into your patrol vehicle before fleeing.
- **Shoot and Flee** – The suspect fires from inside the vehicle, then flees.
- **Spit** – The suspect spits at you from inside their vehicle.
- **Get Out RO (Random Outcome)** – The suspect exits and reaches for something:
  - They may pull a **gun**, a **knife**, or just a **phone**.
  - Gun/knife outcomes each include 2–3 random behavioral variants.

---

## 🛠️ Built With

- C#
- .NET Framework
- RAGEPluginHook
- LSPD:FR
- A smidgen of love

---

## 📚 Docs

- [API Reference](./docs/API.md)
- [How to Contribute](./CONTRIBUTING.md)
- [Architecture Overview](./docs/Architecture.md)


## Reminder
<img width="1000" height="525" alt="image" src="https://github.com/user-attachments/assets/d770d7b6-f391-4655-800d-2b994c568f28" />


---

## 📬 Need Help?

Want to contribute, request features, or need guidance?  
- Join the discussion or message me on Discord: `astro.1181`
