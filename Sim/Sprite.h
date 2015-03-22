#ifndef SPRITE_H
#define SPRITE_H

#include <memory>
#include <vector>
#include <SDL.h>
#include "Graphics.h"

class Sprite
{
public:
	Sprite();
	Sprite(Graphics* graphics, SDL_Texture* texture, int frames, int w, int h);
	~Sprite();

	void addFrame(int x, int y);
	void setFrame(int frame);
	void draw(int x, int y);

private:
	Graphics* m_graphics;
	SDL_Texture* texture_;
	SDL_Rect* m_rect;
	std::vector<SDL_Rect> m_frames;
	int m_frame;
};

#endif