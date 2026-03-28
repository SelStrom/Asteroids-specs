# Quick Task 260328-xag: Summary

**Completed:** 2026-03-29
**Status:** Done

## What Was Fixed

### Root Cause
Previous fix (v8q) left `vfx_blow.prefab` with custom `vfx_blow_mat.mat` using `Sprites/Default`
shader pointing to the full asteroids atlas texture. Particles were invisible because:
- `Sprites/Default` shader not reliable for ParticleSystemRenderer in Built-in RP
- Full atlas texture has transparent regions — random UV sampling = invisible
- `startSize: 0.3` too small in PPU=16 world

### Changes
- `vfx_blow.prefab`: material → built-in `Default-Particle` (fileID 10301, always available)
- `vfx_blow.prefab`: `startSize: 0.3 → 0.8`, `startSpeed: 3 → 5`
- `Phase6Setup.cs`: use `Resources.GetBuiltinResource<Material>("Default-Particle.mat")`

## Result
Explosion effect renders white additive particles (burst of 15) on asteroid/ship destruction.
