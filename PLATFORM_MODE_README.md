# Swazy Platform Mode

## Overview
The Swazy application now supports two operating modes:
1. **Platform Mode**: Shows the Swazy SaaS landing page
2. **Tenant Mode**: Shows the tenant-specific barber shop booking system

## Switching Modes

### To Enable Platform Mode (Landing Page)
Open `swazy-ui/src/environments/environment.development.ts` and set:
```typescript
platformType: 'platform'
```

### To Enable Tenant Mode (Barber Shop)
Open `swazy-ui/src/environments/environment.development.ts` and set:
```typescript
platformType: 'tenant'
```