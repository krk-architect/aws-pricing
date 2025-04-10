# AWS Pricing Calculator for ECS Fargate

A highly configurable console application for computing ECS Fargate costs under various execution scenarios.

Define clusters of tasks by specifying:

- the number of Savings Plan tasks (24 hours/day)
- the number of On Demand tasks (along with the estimated start/end times)

This allows you to tweak various combinations of Savings Plan vs. On Demand tasks accounting for scaling scenarios (e.g., bell curve throughput).

---

## 🚀 Quick Start

### Build and Run (Development)

```bash
dotnet run --output ./Files/Output --config ./Files/Input/SavingsPlanScaling.yml --config ./Files/Input/OnDemandScaling.yml --config ./Files/Input/OnDemand24x7.yml
```

### Publish and Run (Production)

```bash
dotnet publish -c Release -o ./publish/aws-pricing
cd ./publish/aws-pricing
./aws-pricing.exe --output ./Files/Output --config ./Files/Input/SavingsPlanScaling.yml --config ./Files/Input/OnDemandScaling.yml --config ./Files/Input/OnDemand24x7.yml
```

---

## 🧾 Input Configuration

Each configuration file is a YAML document specifying:

- CPU and memory (must match valid Fargate task configurations)
- Task scheduling (start/end times - military time)
- Number of tasks
- Desired pricing model (Savings Plan and/or On-Demand)

### Example Input File

```yaml
region: us-east-2
discounts:
  enterprise: 0.15
  savingsPlan: 0.2
clusters:
  - name: prod
    cpu: 8
    gb: 16
    tasks:
      savingsPlan:
        - tasks: 2
      onDemand:
        - tasks: 4
          hours: [4, 20]
        - tasks: 2
          hours: [8, 16]
```

---

## 📤 Output

The application writes two types of outputs per configuration file:

- **Text**: `./Files/Output/text/*.txt`

  A human-readable report with per-task and per-cluster pricing.

- **JSON**: `./Files/Output/json/*.json`

  Structured pricing data for downstream integration.

Additionally, it prints a summary to the console:

```plaintext
SavingsPlanScaling = $27,041.24    C:\path\to\SavingsPlanScaling.yml
OnDemandScaling    = $32,269.63    C:\path\to\OnDemandScaling.yml
OnDemand24x7       = $42,890.02    C:\path\to\OnDemand24x7.yml
```

---

## 📊 Pricing Logic

- CPU: `$0.04048` per vCPU-hour
- Memory: `$0.004445` per GB-hour
- Discounts:
  - Savings Plan: `20%`
  - Enterprise: `15%` (applied after Savings Plan discount)

Final cost = (Base CPU + Memory cost) × (1 - savingsPlan%) × (1 - enterprise%)

---

## 📁 Project Structure

```plaintext
src/
├── Pricing/
│   ├── Files/            # Example input config files and output files
│   ├── Models/           # Domain models for config, pricing, and Fargate logic
│   ├── Strategies/       # Output format strategies (JSON, text)
│   ├── Visitors/         # Visitor pattern to decouple logic for price, display, etc.
│   ├── App.cs            # Main orchestrator for CLI
│   ├── Program.cs        # Bootstrapper and Host configuration
│   └── aws-pricing.appsettings.json
```

---

Currently limited to:

- Region: `us-east-2` (hardcoded pricing for now)
- Platform: Linux, x86 architecture
- Ephemeral Storage: 20 GB (free tier)
- Pricing Models: On-Demand and 1-year No Upfront Savings Plan

---

## 👤 Author

**Kyle Kolander**
[https://github.com/krk-architect/aws-pricing](https://github.com/krk-architect/aws-pricing)

---

## ⚖ License

© 2025 Kyle. All rights reserved.
