# RiskierTrafficStops
A LSPDFR Plugin to make traffic stops more interesting

## Contributing Information
  - If you want to contribute or use my code please read the license, thanks!
  - If you need help contributing, such as adding new outcomes, don't be afraid to send me a message on discord: astro.1181

## API Documentation for developers
- DisableRTSForCurrentStop
  - Public get/set
  - Disables RTS From interacting with the next/current pullover
  - Reset after every pullover

- DisableRTSForPeds
  - Params:
    - `params Ped[] peds` - Peds to have RTS not interact with
  - All invalid peds are cleared from the list every 10 actively playing minutes
  - If a ped is still valid it is not removed from the list on each pass
  - If one of the supplied peds is the driver, the outcome is ended immediately 

- Events
  - OnRTSOutcomeStarted
    - Invoked every time a RTS Outcome is started
  - OnRTSOutcomeEnded
    - Invoked every time the active RTS Outcome is ended

  *if you feel anything is missing from the documentation, please contact me!*
