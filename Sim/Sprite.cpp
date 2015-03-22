#include "Sprite.h"

using namespace std;

Sprite::Sprite()
{
}

Sprite::Sprite(Graphics* graphics, SDL_Texture* texture, int frames, int w, int h)
{
	m_graphics = graphics;
	texture_ = texture;
	m_rect = new SDL_Rect();
	m_rect->w = w;
	m_rect->h = h;
	m_frames.reserve(frames);
}

Sprite::~Sprite()
{
}

void Sprite::addFrame(int x, int y)
{
	m_frames.push_back({ x, y });
}

void Sprite::setFrame(int frame)
{
	m_frame = frame;
}

void Sprite::draw(int x, int y)
{
	SDL_Rect* r = new SDL_Rect();
	r->x = m_frames[m_frame].x;
	r->y = m_frames[m_frame].y;
	r->w = m_rect->w;
	r->h = m_rect->h;
	m_graphics->renderTexture(&*texture_, x, y, r);
};
