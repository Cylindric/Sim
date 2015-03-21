#include "World.h"

#include <iostream>
#include <string>
#include <fstream>

#include "res_path.h"

using namespace std;

World::World(Graphics& graphics) 
	:m_characterManager(graphics), 
	m_tileSize(40)
{
	this->loadTiles(graphics);
}

World::~World()
{
	//m_graphicsComp->closeTexture(m_tileset);
}


SDL_Rect* World::getSpriteRect(int spriteId)
{
	SDL_Rect* rect = new SDL_Rect();
	switch (spriteId)
	{
	case 0: *rect = { 40, 81, 40, 40 }; break;
	case 1: *rect = { 120, 81, 40, 40 }; break;
	case 2: *rect = { 200, 81, 40, 40 }; break;
	case 3: *rect = { 280, 81, 40, 40 }; break;

	case 4: *rect = { 40, 161, 40, 40 }; break;
	case 5: *rect = { 120, 161, 40, 40 }; break;
	case 6: *rect = { 200, 161, 40, 40 }; break;
	case 7: *rect = { 280, 161, 40, 40 }; break;
	case 8: *rect = { 360, 161, 40, 40 }; break;

	case 9: *rect = { 40, 241, 40, 40 }; break;
	case 10: *rect = { 120, 241, 40, 40 }; break;
	case 11: *rect = { 200, 241, 40, 40 }; break;
	case 12: *rect = { 280, 241, 40, 40 }; break;
	case 13: *rect = { 360, 241, 40, 40 }; break;

		// Debug markers
	case 14: *rect = { 40, 321, 40, 40 }; break;
	case 15: *rect = { 40, 321, 40, 40 }; break;
	}
	return rect;
}

void World::loadTiles(Graphics& graphics)
{
	// Read data from the file
	ifstream fin;

	fin.open(Resource::getResourcePath("Maps") + "map-01.txt");
	if (fin.fail())
	{
		cout << "Error loading map-01.txt" << endl;
		throw;
	}
	
	string word = "";
	string tilefilename = "";
	string width = "";
	string height = "";

	fin >> word >> tilefilename;
	m_tileset = graphics.loadTexture(Resource::getResourcePath("Tiles") + tilefilename);
	if (m_tileset == NULL)
	{
		cout << "Error loading texture " << tilefilename;
		throw;
	}

	fin >> word >> width;
	m_tileColumns = atoi(width.c_str());
	if (m_tileColumns == 0)
	{
		cout << "Error getting width of map. " << width;
		throw;
	}

	fin >> word >> height;
	m_tileRows = atoi(height.c_str());
	if (m_tileRows == 0)
	{
		cout << "Error getting height of map. " << height;
		throw;
	}

	// Get the bulk of the data
	int tileCount = m_tileColumns * m_tileRows;
	m_tiles.reserve(tileCount);

	int spriteToUse = 0;
	int tileNum = 0;
	while (fin >> word)
	{
		int xPos = m_tileSize * (tileNum % m_tileColumns);
		int yPos = m_tileSize * (tileNum / m_tileColumns);
		spriteToUse = atoi(word.c_str());
		bool walkable = (spriteToUse == 0 || spriteToUse == 2 || spriteToUse == 3 || spriteToUse == 6 || spriteToUse == 11);
		m_tiles.emplace_back(m_tileset, this->getSpriteRect(spriteToUse), xPos, yPos, walkable);
		tileNum++;
	}
	fin.close();
}

void World::update(float delta)
{
	m_characterManager.update(delta, this);

	for (Tile& tile : m_tiles)
	{
		tile.update(delta);
	}
}

void World::draw(Graphics& graphics)
{
	int xPos, yPos, tileNum;
	Tile* tile;
	for (int row = 0; row < m_tileRows; row++){
		for (int col = 0; col < m_tileColumns; col++)
		{
			xPos = col * m_tileSize;
			yPos = row * m_tileSize;
			tileNum = (row * m_tileColumns) + col;
			tile = &m_tiles.at(tileNum);
			tile->draw(graphics);
		}
	}

	m_characterManager.draw(graphics);
}

bool World::isWalkable(int x, int y)
{
	int col = x / m_tileSize;
	int row = y / m_tileSize;

	if (col < 0 || row < 0)
	{
		return false;
	}
	if (col > m_tileColumns || row > m_tileRows)
	{
		return false;
	}
	int tileId = (row*m_tileColumns) + col;
	Tile* tile = &m_tiles.at(tileId);
	return tile->isWalkable();
}
