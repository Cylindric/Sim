#include <iostream>
#include "Character.h"

using namespace std;

Character::Character(Graphics* graphics, SDL_Texture* spritesheet, int frames, int w, int h, string name)
{
	m_sprite = new Sprite(graphics, spritesheet, frames, w, h);
	m_name = name;
	m_maxSpeed = 100;
	m_frame = 0;
	m_cycle = 0;
	m_age = 0.0f;
	m_frames = &m_idleDownFrames;
	m_footOffsetX = w / 2;
	m_footOffsetY = h;
}


Character::~Character()
{
	delete m_sprite;
}


void Character::addFrame(int x, int y)
{
	m_sprite->addFrame(x, y);
}


void Character::setIdleFrames(std::vector<int> idleUp, std::vector<int> idleDown, std::vector<int> idleLeft, std::vector<int> idleRight)
{
	m_idleUpFrames = idleUp;
	m_idleDownFrames = idleDown;
	m_idleLeftFrames = idleLeft;
	m_idleRightFrames = idleRight;
}


void Character::setWalkFrames(std::vector<int> walkUp, std::vector<int> walkDown, std::vector<int> walkLeft, std::vector<int> walkRight)
{
	m_walkUpFrames = walkUp;
	m_walkDownFrames = walkDown;
	m_walkLeftFrames = walkLeft;
	m_walkRightFrames = walkRight;
}


void Character::setPosition(int x, int y)
{
	m_posX = float(x);
	m_posY = float(y);
}


void Character::moveLeft()
{
	m_speedX = -m_maxSpeed;
	m_speedY = 0;
	m_frames = &m_walkLeftFrames;
}


void Character::moveRight()
{
	m_speedX = m_maxSpeed;
	m_speedY = 0;
	m_frames = &m_walkRightFrames;
}


void Character::moveUp()
{
	m_speedX = 0;
	m_speedY = -m_maxSpeed;
	m_frames = &m_walkUpFrames;
}


void Character::moveDown()
{
	m_speedX = 0;
	m_speedY = m_maxSpeed;
	m_frames = &m_walkDownFrames;
}


void Character::stopMoving()
{
	if (m_speedX > 0)
	{
		m_frames = &m_idleRightFrames;
	}
	else if (m_speedX < 0)
	{
		m_frames = &m_idleLeftFrames;
	}
	else if (m_speedY > 0)
	{
		m_frames = &m_idleDownFrames;
	}
	else if (m_speedY < 0)
	{
		m_frames = &m_idleUpFrames;
	}

	m_speedX = 0;
	m_speedY = 0;
}


void Character::update(float delta)
{
	float newPosX = m_posX + (m_speedX * delta);
	float newPosY = m_posY + (m_speedY * delta);

	if (m_speedX != 0 || m_speedY != 0)
	{
		//if (m_world->isWalkable(int(newPosX + m_footOffsetX), int(newPosY + m_footOffsetY)))
		//{
		m_posX = newPosX;
		m_posY = newPosY;
		//}
	}
	else
	{
		this->stopMoving();
		//}
	}

	m_age += delta;
	if (m_age > 0.24)
	{
		m_cycle++;
		m_age = 0;
	}

	m_frame = m_frames->at(m_cycle % m_frames->size());

}


void Character::draw()
{
	m_sprite->setFrame(m_frame);
	m_sprite->draw(int(m_posX), int(m_posY));
}
