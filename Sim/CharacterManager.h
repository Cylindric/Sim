#pragma once

#include "Character.h"
#include "Graphics.h"

// Forward declaration for the World object.
class World;

class CharacterManager
{
public:
	CharacterManager(Graphics& graphics);
	~CharacterManager();
	void update(float delta, World* world);
	void draw(Graphics& graphics);
	void handleKeyboardInput(const Uint8* keysHeld);

private:
	void loadCharacters(Graphics& graphics);
	std::vector<Character*> m_characters;

};

