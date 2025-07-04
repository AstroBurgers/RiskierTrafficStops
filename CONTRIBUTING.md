# How to Contribute to RiskierTrafficStops

We welcome pull requests, suggestions, and bug reports! Here’s how to get started:

## 📥 Clone and Build

1. Clone this repo
2. Open `RiskierTrafficStops.sln` in Visual Studio or Rider
3. Set build target to `x64` / `.NET Framework 4.x` (or your target version)
4. Build → Output DLL will be placed in `bin/Debug` or `bin/Release`

---

## 🧩 Add a New Outcome

1. Navigate to `Mod/Outcomes`
2. Create a new `.cs` file (e.g., `ThreatenWithKnife.cs`)
3. Inherit from `Outcome` base class
4. Hook it into the outcome selection system
5. Test your logic and edge cases!

---

## ✅ Contribution Guidelines

- Follow the existing naming/style conventions
- Add summary XML comments where applicable
- Add to the API.md if your changes affect external usage
- Don’t forget to test builds in-game!
- If adding a new outcome, please examine the structure of other similar outcomes, and try to ensure continuity.

## 📜 License

By contributing, you agree to follow the terms in [LICENSE](../LICENSE).
