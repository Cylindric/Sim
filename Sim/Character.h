#pragma once

#include <vector>
#include "SDL.h"
#include "Graphics.h"
#include "Sprite.h"

class Character
{
public:
	Character(Graphics& graphics, SDL_Texture* spritesheet, int frames, int w, int h, std::string name);
	~Character();
	void addFrame(int x, int y);
	void setIdleFrames(std::vector<int> idleUp, std::vector<int> idleDown, std::vector<int> idleLeft, std::vector<int> idleRight);
	void setWalkFrames(std::vector<int> walkUp, std::vector<int> walkDown, std::vector<int> walkLeft, std::vector<int> walkRight);

	void moveLeft();
	void moveRight();
	void moveUp();
	void moveDown();
	void stopMoving();
	void setPosition(int x, int y);

	void update(float delta);
	void draw(Graphics& graphics);

	// Accessors
	std::string getName(){ return m_name; }

private:
	Sprite* m_sprite;
	std::string m_name;
	int m_frame;
	int m_cycle;
	float m_age;
	float m_posX;
	float m_posY;
	float m_maxSpeed;
	float m_speedX;
	float m_speedY;
	int m_footOffsetX;
	int m_footOffsetY;

	std::vector<int>* m_frames;
	std::vector<int> m_idleUpFrames;
	std::vector<int> m_idleDownFrames;
	std::vector<int> m_idleLeftFrames;
	std::vector<int> m_idleRightFrames;
	std::vector<int> m_walkUpFrames;
	std::vector<int> m_walkDownFrames;
	std::vector<int> m_walkRightFrames;
	std::vector<int> m_walkLeftFrames;
};
