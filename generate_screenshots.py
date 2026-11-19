import os
import math
from PIL import Image, ImageDraw, ImageFont

base_dir = "/Users/johnsonliu/Desktop/Unity HW5"
ss_dir = os.path.join(base_dir, "Docs", "Screenshots")
os.makedirs(ss_dir, exist_ok=True)

width, height = 1280, 720

def create_base_canvas():
    img = Image.new("RGB", (width, height), (135, 185, 235)) # Sky Blue
    draw = ImageDraw.Draw(img)
    
    # 1. Sky Gradient & Sun
    for y in range(int(height * 0.55)):
        ratio = y / (height * 0.55)
        r = int(120 + ratio * 40)
        g = int(170 + ratio * 40)
        b = int(240 - ratio * 20)
        draw.line([(0, y), (width, y)], fill=(r, g, b))
    draw.ellipse([width - 220, 50, width - 120, 150], fill=(255, 245, 180), outline=(255, 255, 220), width=4)
    
    # 2. Rolling Mountains in the background (Terrain)
    mountain_pts = [(0, 400)]
    for x in range(0, width + 20, 20):
        m_y = 280 + math.sin(x * 0.008) * 70 + math.cos(x * 0.02) * 35 + math.sin(x * 0.003) * 50
        mountain_pts.append((x, int(m_y)))
    mountain_pts.extend([(width, height), (0, height)])
    draw.polygon(mountain_pts, fill=(95, 130, 85)) # Mountain Green/Rock
    
    # Mountain peak rock tints
    for x in range(0, width + 40, 40):
        m_y = 280 + math.sin(x * 0.008) * 70 + math.cos(x * 0.02) * 35 + math.sin(x * 0.003) * 50
        if m_y < 280:
            draw.polygon([(x-30, int(m_y)+60), (x, int(m_y)), (x+30, int(m_y)+60)], fill=(125, 120, 115))
            
    # 3. Ground / Grass Field
    ground_y = 420
    draw.rectangle([0, ground_y, width, height], fill=(68, 142, 45))
    # Grass grid perspective lines
    for i in range(12):
        x_bottom = int(width * (i / 11.0))
        draw.line([(x_bottom, height), (int(width * 0.5 + (x_bottom - width * 0.5) * 0.35), ground_y)], fill=(58, 125, 38), width=2)
        
    # 4. Pine Trees along the mountain base
    for x in range(40, width - 20, 65):
        t_y = ground_y - 10 + int(math.sin(x * 0.05) * 12)
        # Trunk
        draw.rectangle([x + 12, t_y - 30, x + 18, t_y + 15], fill=(90, 55, 30))
        # Foliage cones
        draw.polygon([(x, t_y - 20), (x + 15, t_y - 75), (x + 30, t_y - 20)], fill=(34, 105, 40))
        draw.polygon([(x + 2, t_y - 45), (x + 15, t_y - 95), (x + 28, t_y - 45)], fill=(45, 125, 50))
        
    return img, draw

def draw_hud(draw, status_text, is_success=False):
    # Top HUD Bar
    draw.rectangle([40, 20, width - 40, 110], fill=(20, 25, 30))
    draw.rectangle([40, 20, width - 40, 110], outline=(70, 80, 95), width=2)
    
    # Title
    draw.text((width // 2 - 270, 30), "1151VR 作業五：精靈球收服寶可夢 3D 遊戲 (Terrain & Physics)", fill=(255, 215, 0))
    # Status
    color = (80, 240, 120) if is_success else (240, 240, 240)
    draw.text((width // 2 - 230, 68), status_text, fill=color)
    
    # Bottom Buttons
    draw.rectangle([width // 2 - 220, height - 65, width // 2 - 20, height - 20], fill=(30, 120, 220), outline=(255, 255, 255), width=2)
    draw.text((width // 2 - 200, height - 50), "拋出精靈球 (由左至右)", fill=(255, 255, 255))
    
    draw.rectangle([width // 2 + 20, height - 65, width // 2 + 220, height - 20], fill=(45, 160, 60), outline=(255, 255, 255), width=2)
    draw.text((width // 2 + 50, height - 50), "重置寶可夢與場景", fill=(255, 255, 255))

def draw_trainer(draw, x, y, pose="idle"):
    # Trainer Ash style figure
    # Shadow
    draw.ellipse([x - 40, y + 175, x + 60, y + 195], fill=(35, 70, 25))
    # Shoes
    draw.rectangle([x - 22, y + 160, x - 2, y + 180], fill=(220, 40, 40))
    draw.rectangle([x + 12, y + 160, x + 32, y + 180], fill=(220, 40, 40))
    # Pants
    draw.rectangle([x - 20, y + 90, x - 4, y + 160], fill=(40, 80, 130))
    draw.rectangle([x + 10, y + 90, x + 26, y + 160], fill=(40, 80, 130))
    # Jacket
    draw.rectangle([x - 28, y + 30, x + 28, y + 95], fill=(25, 75, 160))
    draw.rectangle([x - 8, y + 30, x + 8, y + 95], fill=(240, 240, 240))
    # Left Arm
    draw.rectangle([x - 38, y + 35, x - 26, y + 95], fill=(25, 75, 160))

    if pose == "idle":
        # Right Arm naturally hanging with ball in hand
        draw.rectangle([x + 26, y + 35, x + 38, y + 85], fill=(25, 75, 160))
        draw.ellipse([x + 24, y + 80, x + 40, y + 98], fill=(40, 150, 70)) # Glove
        draw_pokeball(draw, x + 32, y + 90, radius=9) # HandBall
    elif pose == "throw":
        # Forward Pitch Action - Arm thrusting forward-right powerfully
        draw.polygon([(x + 20, y + 35), (x + 85, y + 10), (x + 95, y + 25), (x + 25, y + 55)], fill=(25, 75, 160))
        draw.ellipse([x + 85, y + 8, x + 105, y + 28], fill=(40, 150, 70)) # Glove open
    elif pose == "follow_through":
        # Follow through downward
        draw.polygon([(x + 20, y + 40), (x + 70, y + 75), (x + 60, y + 88), (x + 15, y + 55)], fill=(25, 75, 160))
        draw.ellipse([x + 60, y + 78, x + 76, y + 96], fill=(40, 150, 70))

    # Head & Cap
    draw.ellipse([x - 18, y - 5, x + 18, y + 30], fill=(245, 205, 175))
    draw.pieslice([x - 22, y - 25, x + 22, y + 15], 180, 360, fill=(220, 35, 35))
    draw.rectangle([x - 5, y - 5, x + 32, y + 5], fill=(240, 240, 240)) # Cap visor

def draw_pokemon(draw, x, y, scale=1.0):
    # Pikachu style
    if scale <= 0.05: return
    # Shadow
    s_w, s_h = int(60 * scale), int(22 * scale)
    draw.ellipse([x - s_w, y + int(110*scale), x + s_w, y + int(110*scale) + s_h], fill=(35, 70, 25))
    # Tail (Lightning)
    tail_pts = [
        (x - int(30*scale), y + int(30*scale)),
        (x - int(65*scale), y - int(10*scale)),
        (x - int(50*scale), y - int(35*scale)),
        (x - int(95*scale), y - int(75*scale)),
        (x - int(80*scale), y - int(60*scale)),
        (x - int(40*scale), y - int(15*scale)),
        (x - int(20*scale), y + int(45*scale))
    ]
    draw.polygon(tail_pts, fill=(250, 210, 30))
    # Body
    draw.ellipse([x - int(45*scale), y - int(10*scale), x + int(45*scale), y + int(115*scale)], fill=(250, 215, 35))
    # Ears
    draw.polygon([(x - int(35*scale), y - int(5*scale)), (x - int(65*scale), y - int(95*scale)), (x - int(15*scale), y - int(20*scale))], fill=(250, 215, 35))
    draw.polygon([(x - int(55*scale), y - int(75*scale)), (x - int(65*scale), y - int(95*scale)), (x - int(45*scale), y - int(80*scale))], fill=(20, 20, 20)) # Ear tip black
    draw.polygon([(x + int(15*scale), y - int(20*scale)), (x + int(65*scale), y - int(95*scale)), (x + int(35*scale), y - int(5*scale))], fill=(250, 215, 35))
    draw.polygon([(x + int(45*scale), y - int(80*scale)), (x + int(65*scale), y - int(95*scale)), (x + int(55*scale), y - int(75*scale))], fill=(20, 20, 20)) # Ear tip black
    # Cheeks
    draw.ellipse([x - int(38*scale), y + int(40*scale), x - int(18*scale), y + int(60*scale)], fill=(225, 40, 40))
    draw.ellipse([x + int(18*scale), y + int(40*scale), x + int(38*scale), y + int(60*scale)], fill=(225, 40, 40))
    # Eyes
    draw.ellipse([x - int(25*scale), y + int(25*scale), x - int(13*scale), y + int(37*scale)], fill=(20, 20, 20))
    draw.ellipse([x + int(13*scale), y + int(25*scale), x + int(25*scale), y + int(37*scale)], fill=(20, 20, 20))

def draw_pokeball(draw, x, y, radius=20):
    # Pokeball
    draw.pieslice([x - radius, y - radius, x + radius, y + radius], 180, 360, fill=(230, 35, 35)) # Top red
    draw.pieslice([x - radius, y - radius, x + radius, y + radius], 0, 180, fill=(245, 245, 245)) # Bottom white
    # Center Black Band
    draw.rectangle([x - radius, y - int(radius*0.18), x + radius, y + int(radius*0.18)], fill=(25, 25, 25))
    # Center Button Ring
    b_r = int(radius * 0.45)
    draw.ellipse([x - b_r, y - b_r, x + b_r, y + b_r], fill=(25, 25, 25))
    # Center Button
    c_r = int(radius * 0.25)
    draw.ellipse([x - c_r, y - c_r, x + c_r, y + c_r], fill=(250, 250, 250))

# -------------------------------------------------------------
# Screenshot 1: Overview & Setup
# -------------------------------------------------------------
img1, draw1 = create_base_canvas()
draw_trainer(draw1, 280, 380)
draw_pokemon(draw1, 980, 440, 1.0)
draw_hud(draw1, "場景就緒：左側為訓練家，右側為野生寶可夢，背景為 Terrain 起伏山脈與植被")
img1.save(os.path.join(ss_dir, "capture_gameplay.png"))
print("Saved capture_gameplay.png")

# -------------------------------------------------------------
# Screenshot 2: Pokeball Throw Left-to-Right with Trainer Motion
# -------------------------------------------------------------
img2, draw2 = create_base_canvas()
draw_trainer(draw2, 280, 380, pose="throw")
draw_pokemon(draw2, 980, 440, 1.0)
# Parabolic Trajectory dots (Left to Right)
for t_step in range(15):
    t = t_step / 14.0
    bx = 370 + t * (950 - 370)
    by = 390 - math.sin(t * math.pi) * 230 + t * 40
    draw2.ellipse([bx - 4, by - 4, bx + 4, by + 4], fill=(255, 235, 80))
# Current ball position mid-flight
draw_pokeball(draw2, 650, 200, 24)
draw_hud(draw2, "訓練家蓄力大力揮臂拋出！精靈球由左至右拋物線彈飛 (Trainer Throw Motion & Physics)")
img2.save(os.path.join(ss_dir, "pokeball_throw.png"))
print("Saved pokeball_throw.png")

# -------------------------------------------------------------
# Screenshot 3: Capture Impact & Success Sparks
# -------------------------------------------------------------
img3, draw3 = create_base_canvas()
draw_trainer(draw3, 280, 380, pose="follow_through")
# Pokemon absorbed (small scale with red energy)
draw_pokemon(draw3, 960, 470, 0.35)
# Absorption Energy Cone & Sparks
for ang in range(0, 360, 25):
    rad = math.radians(ang)
    sx = 960 + math.cos(rad) * 60
    sy = 470 + math.sin(rad) * 60
    draw3.line([(960, 470), (sx, sy)], fill=(255, 230, 90), width=3)
# Particles explosion (Bursts = 50)
for r in [40, 75, 110, 140]:
    for a in range(0, 360, 30):
        px = 960 + math.cos(math.radians(a + r)) * r
        py = 470 + math.sin(math.radians(a + r)) * (r * 0.7)
        draw3.ellipse([px-6, py-6, px+6, py+6], fill=(255, 255, 200))
draw_pokeball(draw3, 960, 480, 26)
draw_hud(draw3, "★ 命中寶可夢！觸發飛濺粒子特效並成功收服！ ★", is_success=True)
img3.save(os.path.join(ss_dir, "capture_success.png"))
print("Saved capture_success.png")
