#pragma once

#include <vector>
#include "SDL.h"
#include "Graphics.h"
#include "GraphicsComponent.h"
#include "Tile.h"
#include "CharacterManager.h"

class World
{
public:
	// Constructors
	World(Graphics& graphics);
	~World();

	// Public Methods
	void update(float delta);
	void draw(Graphics& graphics);
	bool checkCollision(SDL_Rect* hitbox);

	// Accessors
	int getWidth() { return m_tileColumns * m_tileSize; }
	int getHeight() { return m_tileRows * m_tileSize; }

private:
	// Private Methods
	void loadTiles(Graphics& graphics);

	SDL_Rect* getSpriteRect(int spriteId);

	// Private objects
	GraphicsComponent m_graphicsComp;
	SDL_Texture* tileset_;
	std::vector<Tile> m_tiles;
	CharacterManager m_characterManager;
	int m_tileSize;
	int m_tileColumns;
	int m_tileRows;
};