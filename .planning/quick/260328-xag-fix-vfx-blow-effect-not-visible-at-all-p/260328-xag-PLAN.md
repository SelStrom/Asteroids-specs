# Quick Task 260328-xag: Fix vfx_blow effect not visible at all — ParticleSystem not rendering

**Date:** 2026-03-28
**Status:** Complete

## Problem

After previous fix (v8q), `vfx_blow.prefab` got `m_Materials: {fileID: 0}` (null). My follow-up
created `vfx_blow_mat.mat` using `Sprites/Default` shader (fileID 10757) referencing the full
asteroids atlas texture. This caused invisible particles because:
1. The "Sprites/Default" shader may not work correctly for ParticleSystemRenderer
2. The full atlas texture has many transparent areas — particles sampling random UVs = invisible
3. `startSize: 0.3` too small for PPU=16 world scale

## Fix

**Task 1: Use built-in Default-Particle material in prefab**
- `Assets/Media/prefabs/vfx_blow.prefab` → `m_Materials: {fileID: 10301, guid: 0000...0, type: 0}`
- Unity's Default-Particle is always available, renders white additive particles

**Task 2: Increase particle parameters for visibility**
- `startSize: 0.3 → 0.8` world units (visible in PPU=16 world)
- `startSpeed: 3 → 5` for more spread

**Task 3: Fix Phase6Setup.cs to match**
- Use `Resources.GetBuiltinResource<Material>("Default-Particle.mat")` instead of custom material
- Remove now-unused `VfxBlowMatPath` constant
