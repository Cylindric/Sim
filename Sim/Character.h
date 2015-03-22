#pragma once

#include <memory>
#include <vector>
#include "SDL.h"
#include "Graphics.h"
#include "Sprite.h"
#include "Input.h"

// Forward declaration for the World object.
class World;

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
	void setPosition(float x, float y);

	void update(float delta, World* world);
	void draw(Graphics& graphics);

	// Accessors
	std::string getName(){ return m_name; }

private:
	Input m_input;
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
	std::shared_ptr<SDL_Rect> m_hitbox;

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
