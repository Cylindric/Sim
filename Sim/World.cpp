#include "World.h"

#include <iostream>
#include <string>
#include <fstream>

using namespace std;

World::World(Graphics* graphics)
{
	m_tileSize = 40;
	m_graphics = graphics;
	this->loadTiles();
	this->loadSprites();
}

World::~World()
{
	m_graphics->closeTexture(m_tileset);
}


void World::loadSprites()
{
	int numTiles = 14;
	m_sprites = new vector < Tile >();
	m_sprites->reserve(numTiles);
	
	// The basic demo spritesheet has the tiles all over the place
	m_sprites->emplace_back(m_graphics, m_tileset, 40, 81, 40, 40, true);
	m_sprites->emplace_back(m_graphics, m_tileset, 120, 81, 40, 40, false);
	m_sprites->emplace_back(m_graphics, m_tileset, 200, 81, 40, 40, true);
	m_sprites->emplace_back(m_graphics, m_tileset, 280, 81, 40, 40, true);

	m_sprites->emplace_back(m_graphics, m_tileset, 40, 161, 40, 40, false);
	m_sprites->emplace_back(m_graphics, m_tileset, 120, 161, 40, 40, false);
	m_sprites->emplace_back(m_graphics, m_tileset, 200, 161, 40, 40, true);
	m_sprites->emplace_back(m_graphics, m_tileset, 280, 161, 40, 40, false);
	m_sprites->emplace_back(m_graphics, m_tileset, 360, 161, 40, 40, false);

	m_sprites->emplace_back(m_graphics, m_tileset, 40, 241, 40, 40, false);
	m_sprites->emplace_back(m_graphics, m_tileset, 120, 241, 40, 40, false);
	m_sprites->emplace_back(m_graphics, m_tileset, 200, 241, 40, 40, true);
	m_sprites->emplace_back(m_graphics, m_tileset, 280, 241, 40, 40, false);
	m_sprites->emplace_back(m_graphics, m_tileset, 360, 241, 40, 40, false);

	// Debug markers
	m_sprites->emplace_back(m_graphics, m_tileset, 40, 321, 40, 40, false); // 14
	m_sprites->emplace_back(m_graphics, m_tileset, 40, 321, 40, 40, false); // 15

}

void World::loadTiles()
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
	m_tileset = m_graphics->loadTexture(Resource::getResourcePath("Tiles") + tilefilename);
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
	m_tiles = new vector < int >();
	m_tiles->reserve(tileCount);

	int spriteToUse;
	while (fin >> word)
	{
		spriteToUse = atoi(word.c_str());
		m_tiles->push_back(spriteToUse);
	}
	fin.close();
}

void World::draw()
{
	int xPos, yPos, tileNum, sprite;
	for (int row = 0; row < m_tileRows; row++){
		for (int col = 0; col < m_tileColumns; col++)
		{
			xPos = col * m_tileSize;
			yPos = row * m_tileSize;
			tileNum = (row * m_tileColumns) + col;
			sprite = m_tiles->at(tileNum);
			m_sprites->at(sprite).draw(xPos, yPos);
		}
	}
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
	int sprite = m_tiles->at((row*m_tileColumns)+col);
	return m_sprites->at(sprite).isWalkable();
}


