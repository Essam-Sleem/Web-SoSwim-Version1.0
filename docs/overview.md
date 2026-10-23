# SOSWIM (SOS + SWIM) — Project Overview

## 1. Project Concept & Vision
**SOSWIM** is an intelligent marine safety wearable system designed to drastically reduce response times for drowning prevention and open water swimmer distress in oceans and beaches.

### Current Phase: Prototype V1.0
The project is currently in the interactive prototype phase, demonstrating how wearable biosensors interface with beach lifeguard towers and provide a seamless web companion for families.

## 2. Core Functional Requirements
1. **Landing & Showcase Page (`/Home/Index`)**:
   - High-impact, responsive maritime presentation introducing the wearable device and biosensors.
   - Promotion of the lifeguard companion mobile application (App Store / Google Play).
   - Instant tracking access card for families to enter the session code provided by the lifeguard.
   - Pre-configured demo codes for fast prototype testing (`12344321`, `87654321`, `11223344`).

2. **Live Swimmer Telemetry Dashboard (`/Tracking/Index`)**:
   - Real-time monitoring of vitals: Heart Rate (BPM), motion stability, distance from shore, depth, and battery level.
   - Visual Sonar Radar mapping showing the swimmer's position relative to the beach and the assigned lifeguard tower.
   - **The 50% Instability Constraint**: The parent distress button is strictly locked unless sensor indicators report $\ge 50\%$ instability risk, preventing false alarms while ensuring immediate emergency response when risk is genuine.
   - **Multi-Swimmer Tracking**: Ability to monitor multiple children simultaneously by entering additional codes provided by the lifeguard into the same session.
   - **Interactive Scenario Switcher**: Prototype bar enabling instantaneous switching between Stable, Warning (68%), and Danger (94%) scenarios for testing and demonstration.
