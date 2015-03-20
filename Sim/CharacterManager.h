#pragma once

#include "World.h"
#include "Character.h"
#include "Graphics.h"

class CharacterManager
{
public:
	CharacterManager(Graphics* graphics, World* world);
	~CharacterManager();
	void update(float delta);
	void draw();
	void handleKeyboardInput(const Uint8* keysHeld);

private:
	void loadCharacters();

	Graphics* m_graphics;
	World* m_world;
	std::vector<Character*> m_characters;

};

