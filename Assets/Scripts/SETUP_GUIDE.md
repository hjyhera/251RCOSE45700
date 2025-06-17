# Bomberman Tilemap Setup Guide

## Issues Fixed:
1. **Map changes when hitting play** - MapSynchronizer now has `autoSynchronizeOnStart` option
2. **Bomb explosions not destroying tiles** - Fixed BombController logic
3. **Player movement issues** - Added proper tilemap collision setup

## Setup Steps:

### 1. MapSynchronizer Setup
- **Important**: Set `autoSynchronizeOnStart = false` to prevent map changes on play
- Only enable this if you want the map to regenerate automatically
- Use the editor buttons to manually control tile generation

### 2. Player Setup
- Add `BombermanGameSetup` script to your scene
- Assign both tilemaps in the script
- Assign all player GameObjects
- This will automatically setup colliders and references

### 3. BombController Setup (for each player)
- Assign `indestructibleTiles` reference
- Assign `destructibleTiles` reference  
- Set `itemSpawnChance` (0-1)
- Assign `spawnableItems` array with power-up prefabs
- Set `explosionLayerMask` to include Stage layer

### 4. Layer Setup
- Ensure tilemaps are on "Stage" layer
- Players should be on "Player" layer
- Bombs should be on "Bomb" layer
- Explosions should be on "Explosion" layer

### 5. Tilemap Collider Setup
- `BombermanGameSetup` will automatically add:
  - TilemapCollider2D
  - CompositeCollider2D
  - Rigidbody2D (Static)
- This creates proper physics collision

### 6. Movement System Choice
**Option A: Keep existing MovementController**
- Uses physics-based movement
- Works with tilemap colliders
- No changes needed

**Option B: Use new TilemapMovementController**
- Direct tilemap collision checking
- Grid-based or smooth movement
- Better performance
- Assign tilemap references

## Testing Checklist:

### Map Generation:
- [ ] MapSynchronizer doesn't auto-generate on play
- [ ] "Synchronize Map Data" reads indestructible tilemap correctly
- [ ] "Generate Destructible Tiles" places tiles in empty spaces only
- [ ] "Clear Destructible Tiles" removes all destructible tiles

### Player Movement:
- [ ] Players can move around the map
- [ ] Players collide with indestructible tiles
- [ ] Players collide with destructible tiles
- [ ] Players can't move through walls

### Bomb System:
- [ ] Players can place bombs
- [ ] Bombs explode after fuse time
- [ ] Explosions stop at indestructible tiles
- [ ] Explosions destroy destructible tiles
- [ ] Items spawn when destructible tiles are destroyed
- [ ] Players can pick up items

### Collision Layers:
- [ ] Indestructible tilemap on "Stage" layer
- [ ] Destructible tilemap on "Stage" layer
- [ ] Players on "Player" layer
- [ ] Bombs on "Bomb" layer
- [ ] Explosions on "Explosion" layer

## Debug Tips:

1. **Check Console Messages**: The scripts log detailed information
2. **Tilemap Bounds**: Use TilemapSizeReader to verify bounds
3. **Collision Visualization**: Enable Gizmos in Scene view
4. **Layer Mask Issues**: Verify LayerMask settings in Physics2D settings

## Common Issues:

### Map keeps changing on play:
- Set `MapSynchronizer.autoSynchronizeOnStart = false`

### Bombs don't destroy tiles:
- Check tilemap references in BombController
- Verify destructible tilemap has tiles
- Check explosion layermask includes Stage

### Players can't move:
- Check tilemap colliders are setup
- Verify layer assignments
- Use BombermanGameSetup script

### Items don't spawn:
- Check itemSpawnChance > 0
- Verify spawnableItems array has prefabs
- Check item prefabs have proper colliders and ItemPickup script
