#include "CharacterManager.h"
using namespace std;

CharacterManager::CharacterManager(Graphics* graphics, World* world)
{
	m_graphics = graphics;
	m_world = world;
	this->loadCharacters();
}


CharacterManager::~CharacterManager()
{
	m_characters.clear();
}


void CharacterManager::loadCharacters()
{
	// Load the character sprite texture sheet
	SDL_Texture *spritesheet = m_graphics->loadTexture(Resource::getResourcePath("Sprites") + "people.png");

	// Create a sprite for the old man
	Character* man = new Character(m_graphics, spritesheet, 12, 32, 32, "Man");
	for (int row = 4; row <= 7; row++){
		for (int col = 3; col <= 5; col++){
			man->addFrame(col * 32, row * 32);
		}
	}
	Character* girl = new Character(m_graphics, spritesheet, 12, 32, 32, "Girl");
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
	man->setPosition(2*(m_world->getWidth() / 3), m_world->getHeight()/ 2);
	girl->setIdleFrames(idleUpFrames, idleDownFrames, idleLeftFrames, idleRightFrames);
	girl->setWalkFrames(walkUpFrames, walkDownFrames, walkLeftFrames, walkRightFrames);
	girl->setPosition(m_world->getWidth() / 3, m_world->getHeight() / 2);

	m_characters.push_back(man);
	m_characters.push_back(girl);
}

void CharacterManager::update(float delta)
{
	for (const auto &character : m_characters) // access by reference to avoid copying
	{
		character->update(delta);
	}
}

void CharacterManager::draw()
{
	for (const auto &character: m_characters) // access by reference to avoid copying
	{
		character->draw();
	}
}

void CharacterManager::handleKeyboardInput(const Uint8* keysHeld)
{
	if (keysHeld[SDL_SCANCODE_LEFT])
	{
		m_characters[0]->moveLeft();
	}
	else if (keysHeld[SDL_SCANCODE_RIGHT])
	{
		m_characters[0]->moveRight();
	}
	else if (keysHeld[SDL_SCANCODE_UP])
	{
		m_characters[0]->moveUp();
	}
	else if (keysHeld[SDL_SCANCODE_DOWN])
	{
		m_characters[0]->moveDown();
	}
	else
	{
		m_characters[0]->stopMoving();
	}


	if (keysHeld[SDL_SCANCODE_A])
	{
		m_characters[1]->moveLeft();
	}
	else if (keysHeld[SDL_SCANCODE_D])
	{
		m_characters[1]->moveRight();
	}
	else if (keysHeld[SDL_SCANCODE_W])
	{
		m_characters[1]->moveUp();
	}
	else if (keysHeld[SDL_SCANCODE_S])
	{
		m_characters[1]->moveDown();
	}
	else
	{
		m_characters[1]->stopMoving();
	}

}