#include "CharacterManager.h"

#include "res_path.h"
#include <memory>

using namespace std;

CharacterManager::CharacterManager(Graphics& graphics)
{
	this->loadCharacters(graphics);
}


CharacterManager::~CharacterManager()
{
	m_characters.clear();
}


void CharacterManager::loadCharacters(Graphics& graphics)
{
	// Load the character sprite texture sheet
	auto spritesheet = graphics.loadTexture(Resource::getResourcePath("Sprites") + "people.png");

	// Create a sprite for the old man
	Character* man = new Character(graphics, spritesheet, 12, 32, 32, "Man");
	for (int row = 4; row <= 7; row++){
		for (int col = 3; col <= 5; col++){
			man->addFrame(col * 32, row * 32);
		}
	}
	Character* girl = new Character(graphics, spritesheet, 12, 32, 32, "Girl");
	for (int row = 4; row <= 7; row++){
		for (int col = 6; col <= 8; col++){
			girl->addFrame(col * 32, row * 32);
		}
	}
	std::vector<int> idleUpFrames = { 10 };
	std::vector<int> idleDownFrames = { 1 };
	std::vector<int> idleLeftFrames = { 4 };
	std::vector<int> idleRightFrames = { 7 };
	std::vector<int> walkUpFrames = { 9, 11 };
	std::vector<int> walkDownFrames = { 0, 2 };
	std::vector<int> walkLeftFrames = { 3, 5 };
	std::vector<int> walkRightFrames = { 6, 8 };

	man->setIdleFrames(idleUpFrames, idleDownFrames, idleLeftFrames, idleRightFrames);
	man->setWalkFrames(walkUpFrames, walkDownFrames, walkLeftFrames, walkRightFrames);
	man->setPosition(100, 100);
	girl->setIdleFrames(idleUpFrames, idleDownFrames, idleLeftFrames, idleRightFrames);
	girl->setWalkFrames(walkUpFrames, walkDownFrames, walkLeftFrames, walkRightFrames);
	girl->setPosition(150, 150);

	m_characters.push_back(man);
	m_characters.push_back(girl);
}

void CharacterManager::update(float delta, World* world)
{
	for (const auto &character : m_characters)
	{
		character->update(delta, world);
	}
}

void CharacterManager::draw(Graphics& graphics)
{
	for (const auto &character: m_characters)
	{
		character->draw(graphics);
	}
}
