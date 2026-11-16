import os
import math
import struct
import wave
import random
from PIL import Image, ImageDraw

base_dir = "/Users/johnsonliu/Desktop/Unity HW5"
models_dir = os.path.join(base_dir, "Assets", "Models")
audio_dir = os.path.join(base_dir, "Assets", "Audio")
textures_dir = os.path.join(base_dir, "Assets", "Textures")

os.makedirs(models_dir, exist_ok=True)
os.makedirs(audio_dir, exist_ok=True)
os.makedirs(textures_dir, exist_ok=True)

# ==============================================================================
# 1. 3D Model Generation (OBJ Format)
# ==============================================================================

def write_obj_file(filepath, groups):
    """
    groups: list of dict(name=str, vertices=list of (x,y,z), faces=list of 1-based vertex indices list)
    """
    with open(filepath, "w", encoding="utf-8") as f:
        f.write(f"# Exported 3D Model for Unity HW5: {os.path.basename(filepath)}\n")
        vertex_offset = 0
        for group in groups:
            f.write(f"g {group['name']}\n")
            f.write(f"o {group['name']}\n")
            for v in group['vertices']:
                f.write(f"v {v[0]:.4f} {v[1]:.4f} {v[2]:.4f}\n")
            for face in group['faces']:
                f.write("f " + " ".join(str(idx + vertex_offset) for idx in face) + "\n")
            vertex_offset += len(group['vertices'])

def create_sphere(center=(0,0,0), radius=1.0, lat_steps=16, lon_steps=24, y_min=-1.0, y_max=1.0):
    vertices = []
    faces = []
    # Generates a sphere sliced between y_min and y_max normalized
    # normalized range [-1, 1]
    valid_lats = []
    for i in range(lat_steps + 1):
        v = i / lat_steps
        theta = -math.pi / 2 + math.pi * v
        y_norm = math.sin(theta)
        if y_min <= y_norm <= y_max:
            valid_lats.append(theta)
    
    if len(valid_lats) < 2:
        return vertices, faces
    
    for theta in valid_lats:
        cos_theta = math.cos(theta)
        sin_theta = math.sin(theta)
        for j in range(lon_steps):
            phi = 2 * math.pi * j / lon_steps
            x = center[0] + radius * cos_theta * math.sin(phi)
            y = center[1] + radius * sin_theta
            z = center[2] + radius * cos_theta * math.cos(phi)
            vertices.append((x, y, z))
            
    num_lats = len(valid_lats)
    for i in range(num_lats - 1):
        for j in range(lon_steps):
            next_j = (j + 1) % lon_steps
            v1 = i * lon_steps + j + 1
            v2 = i * lon_steps + next_j + 1
            v3 = (i + 1) * lon_steps + next_j + 1
            v4 = (i + 1) * lon_steps + j + 1
            faces.append([v1, v2, v3, v4])
            
    return vertices, faces

def create_cylinder(center=(0,0,0), radius_bottom=0.5, radius_top=0.5, height=1.0, steps=16):
    vertices = []
    faces = []
    y_b = center[1] - height / 2
    y_t = center[1] + height / 2
    for j in range(steps):
        phi = 2 * math.pi * j / steps
        x_b = center[0] + radius_bottom * math.cos(phi)
        z_b = center[2] + radius_bottom * math.sin(phi)
        vertices.append((x_b, y_b, z_b))
    for j in range(steps):
        phi = 2 * math.pi * j / steps
        x_t = center[0] + radius_top * math.cos(phi)
        z_t = center[2] + radius_top * math.sin(phi)
        vertices.append((x_t, y_t, z_t))
    # side faces
    for j in range(steps):
        next_j = (j + 1) % steps
        v1 = j + 1
        v2 = next_j + 1
        v3 = steps + next_j + 1
        v4 = steps + j + 1
        faces.append([v1, v2, v3, v4])
    # bottom center & top center
    v_b_center = len(vertices) + 1
    vertices.append((center[0], y_b, center[2]))
    v_t_center = len(vertices) + 1
    vertices.append((center[0], y_t, center[2]))
    for j in range(steps):
        next_j = (j + 1) % steps
        faces.append([v_b_center, next_j + 1, j + 1])
        faces.append([v_t_center, steps + j + 1, steps + next_j + 1])
    return vertices, faces

def create_box(min_pt, max_pt):
    x0, y0, z0 = min_pt
    x1, y1, z1 = max_pt
    vertices = [
        (x0, y0, z0), (x1, y0, z0), (x1, y1, z0), (x0, y1, z0), # Front
        (x0, y0, z1), (x1, y0, z1), (x1, y1, z1), (x0, y1, z1)  # Back
    ]
    faces = [
        [1, 2, 3, 4], # Front
        [6, 5, 8, 7], # Back
        [5, 1, 4, 8], # Left
        [2, 6, 7, 3], # Right
        [4, 3, 7, 8], # Top
        [5, 6, 2, 1]  # Bottom
    ]
    return vertices, faces

# --- Pokeball Model ---
def generate_pokeball(filepath):
    groups = []
    # 1. Upper Hemisphere (Red)
    v_top, f_top = create_sphere(center=(0,0,0), radius=0.4, lat_steps=18, lon_steps=24, y_min=0.08, y_max=1.0)
    groups.append(dict(name="Pokeball_UpperRed", vertices=v_top, faces=f_top))
    
    # 2. Lower Hemisphere (White)
    v_bot, f_bot = create_sphere(center=(0,0,0), radius=0.4, lat_steps=18, lon_steps=24, y_min=-1.0, y_max=-0.08)
    groups.append(dict(name="Pokeball_LowerWhite", vertices=v_bot, faces=f_bot))
    
    # 3. Middle Belt (Black Ring)
    v_mid, f_mid = create_sphere(center=(0,0,0), radius=0.395, lat_steps=18, lon_steps=24, y_min=-0.1, y_max=0.1)
    groups.append(dict(name="Pokeball_MiddleBand", vertices=v_mid, faces=f_mid))
    
    # 4. Front Button Outer Ring (Black)
    v_ring, f_ring = create_cylinder(center=(0, 0, 0.38), radius_bottom=0.12, radius_top=0.12, height=0.04, steps=16)
    groups.append(dict(name="Pokeball_ButtonRing", vertices=v_ring, faces=f_ring))
    
    # 5. Front Button Center (White)
    v_btn, f_btn = create_cylinder(center=(0, 0, 0.40), radius_bottom=0.07, radius_top=0.07, height=0.03, steps=16)
    groups.append(dict(name="Pokeball_ButtonCenter", vertices=v_btn, faces=f_btn))
    
    write_obj_file(filepath, groups)
    print(f"Generated Pokeball model: {filepath}")

# --- Pokemon (Pikachu) Model ---
def generate_pokemon(filepath):
    groups = []
    # 1. Main Chubby Body (Yellow)
    v_body, f_body = create_sphere(center=(0, 0.6, 0), radius=0.6, lat_steps=16, lon_steps=20, y_min=-0.95, y_max=0.95)
    # Stretch body slightly in Y and Z for adorable pear shape
    v_body_scaled = [(x * 0.9, y * 1.15, z * 0.95) for x, y, z in v_body]
    groups.append(dict(name="Pokemon_BodyYellow", vertices=v_body_scaled, faces=f_body))
    
    # 2. Left Ear (Yellow Base)
    v_ear_l, f_ear_l = create_cylinder(center=(-0.35, 1.45, 0.05), radius_bottom=0.12, radius_top=0.04, height=0.55, steps=12)
    # Slant left ear outward
    v_ear_l_slanted = [(x - (y-1.2)*0.4, y, z) for x, y, z in v_ear_l]
    groups.append(dict(name="Pokemon_LeftEarYellow", vertices=v_ear_l_slanted, faces=f_ear_l))
    
    # 3. Right Ear (Yellow Base)
    v_ear_r, f_ear_r = create_cylinder(center=(0.35, 1.45, 0.05), radius_bottom=0.12, radius_top=0.04, height=0.55, steps=12)
    # Slant right ear outward
    v_ear_r_slanted = [(x + (y-1.2)*0.4, y, z) for x, y, z in v_ear_r]
    groups.append(dict(name="Pokemon_RightEarYellow", vertices=v_ear_r_slanted, faces=f_ear_r))
    
    # 4. Ear Tips (Black)
    v_tip_l, f_tip_l = create_sphere(center=(-0.52, 1.70, 0.05), radius=0.07, lat_steps=8, lon_steps=10)
    groups.append(dict(name="Pokemon_EarTipLeftBlack", vertices=v_tip_l, faces=f_tip_l))
    v_tip_r, f_tip_r = create_sphere(center=(0.52, 1.70, 0.05), radius=0.07, lat_steps=8, lon_steps=10)
    groups.append(dict(name="Pokemon_EarTipRightBlack", vertices=v_tip_r, faces=f_tip_r))
    
    # 5. Red Cheeks (Electric Pouches)
    v_chk_l, f_chk_l = create_sphere(center=(-0.38, 0.75, 0.45), radius=0.14, lat_steps=8, lon_steps=10)
    groups.append(dict(name="Pokemon_CheekLeftRed", vertices=v_chk_l, faces=f_chk_l))
    v_chk_r, f_chk_r = create_sphere(center=(0.38, 0.75, 0.45), radius=0.14, lat_steps=8, lon_steps=10)
    groups.append(dict(name="Pokemon_CheekRightRed", vertices=v_chk_r, faces=f_chk_r))
    
    # 6. Eyes (Black)
    v_eye_l, f_eye_l = create_sphere(center=(-0.22, 0.92, 0.52), radius=0.08, lat_steps=8, lon_steps=8)
    groups.append(dict(name="Pokemon_EyeLeftBlack", vertices=v_eye_l, faces=f_eye_l))
    v_eye_r, f_eye_r = create_sphere(center=(0.22, 0.92, 0.52), radius=0.08, lat_steps=8, lon_steps=8)
    groups.append(dict(name="Pokemon_EyeRightBlack", vertices=v_eye_r, faces=f_eye_r))
    
    # 7. Lightning Bolt Tail (Zigzag segmented)
    tail_pts = [
        ((-0.08, 0.25, -0.5), (0.08, 0.45, -0.65)),
        ((-0.12, 0.40, -0.65), (0.12, 0.70, -0.85)),
        ((-0.18, 0.65, -0.85), (0.18, 1.05, -1.05)),
        ((-0.25, 0.95, -1.05), (0.25, 1.40, -1.25)),
    ]
    v_tail_all = []
    f_tail_all = []
    for min_p, max_p in tail_pts:
        v_box, f_box = create_box(min_p, max_p)
        v_offset = len(v_tail_all)
        v_tail_all.extend(v_box)
        for f in f_box:
            f_tail_all.append([idx + v_offset for idx in f])
    groups.append(dict(name="Pokemon_TailYellow", vertices=v_tail_all, faces=f_tail_all))
    
    # 8. Paws & Feet
    v_paw_l, f_paw_l = create_sphere(center=(-0.25, 0.55, 0.50), radius=0.12, lat_steps=8, lon_steps=10)
    groups.append(dict(name="Pokemon_PawLeftYellow", vertices=v_paw_l, faces=f_paw_l))
    v_paw_r, f_paw_r = create_sphere(center=(0.25, 0.55, 0.50), radius=0.12, lat_steps=8, lon_steps=10)
    groups.append(dict(name="Pokemon_PawRightYellow", vertices=v_paw_r, faces=f_paw_r))
    v_foot_l, f_foot_l = create_sphere(center=(-0.30, 0.10, 0.25), radius=0.16, lat_steps=8, lon_steps=10)
    groups.append(dict(name="Pokemon_FootLeftYellow", vertices=v_foot_l, faces=f_foot_l))
    v_foot_r, f_foot_r = create_sphere(center=(0.30, 0.10, 0.25), radius=0.16, lat_steps=8, lon_steps=10)
    groups.append(dict(name="Pokemon_FootRightYellow", vertices=v_foot_r, faces=f_foot_r))
    
    write_obj_file(filepath, groups)
    print(f"Generated Pokemon model: {filepath}")

# --- Trainer Model (Ash Ketchum style) ---
def generate_trainer(filepath):
    groups = []
    # 1. Trainer Cap (Red dome + White front + Visor)
    v_cap, f_cap = create_sphere(center=(0, 1.82, 0), radius=0.28, lat_steps=12, lon_steps=16, y_min=0.0, y_max=1.0)
    groups.append(dict(name="Trainer_CapRed", vertices=v_cap, faces=f_cap))
    # Visor sticking forward
    v_visor, f_visor = create_box((-0.24, 1.76, 0.15), (0.24, 1.81, 0.45))
    groups.append(dict(name="Trainer_CapVisorRed", vertices=v_visor, faces=f_visor))
    
    # 2. Head / Face (Skin tone)
    v_head, f_head = create_sphere(center=(0, 1.68, 0.05), radius=0.24, lat_steps=12, lon_steps=16)
    groups.append(dict(name="Trainer_HeadSkin", vertices=v_head, faces=f_head))
    
    # 3. Jacket / Torso (Blue Vest with White Collar)
    v_torso, f_torso = create_cylinder(center=(0, 1.25, 0), radius_bottom=0.28, radius_top=0.34, height=0.65, steps=16)
    groups.append(dict(name="Trainer_JacketBlue", vertices=v_torso, faces=f_torso))
    
    # 4. Left Arm (Poised at side / balancing)
    v_arm_l, f_arm_l = create_cylinder(center=(-0.42, 1.15, -0.05), radius_bottom=0.08, radius_top=0.09, height=0.55, steps=12)
    groups.append(dict(name="Trainer_LeftArmBlue", vertices=v_arm_l, faces=f_arm_l))
    
    # 5. Right Arm (Throwing pose pointing towards Pokemon on the right!)
    # Slanted outward and forward
    v_arm_r, f_arm_r = create_cylinder(center=(0.48, 1.35, 0.25), radius_bottom=0.08, radius_top=0.09, height=0.60, steps=12)
    # Rotate arm pointing forward-right
    v_arm_r_posed = [(x + (y-1.35)*0.5, y, z + (y-1.35)*0.4) for x, y, z in v_arm_r]
    groups.append(dict(name="Trainer_RightArmThrowing", vertices=v_arm_r_posed, faces=f_arm_r))
    
    # 6. Hand Glove (Green trainer glove)
    v_glove, f_glove = create_sphere(center=(0.65, 1.50, 0.45), radius=0.10, lat_steps=8, lon_steps=10)
    groups.append(dict(name="Trainer_ThrowingGloveGreen", vertices=v_glove, faces=f_glove))
    
    # 7. Pants / Legs (Teal/Blue Jeans)
    v_leg_l, f_leg_l = create_cylinder(center=(-0.16, 0.50, 0), radius_bottom=0.12, radius_top=0.14, height=0.85, steps=12)
    groups.append(dict(name="Trainer_PantsLegLeft", vertices=v_leg_l, faces=f_leg_l))
    v_leg_r, f_leg_r = create_cylinder(center=(0.16, 0.50, 0.05), radius_bottom=0.12, radius_top=0.14, height=0.85, steps=12)
    groups.append(dict(name="Trainer_PantsLegRight", vertices=v_leg_r, faces=f_leg_r))
    
    # 8. Shoes / Sneakers (Red/Black)
    v_shoe_l, f_shoe_l = create_box((-0.26, 0.0, -0.12), (-0.06, 0.16, 0.20))
    groups.append(dict(name="Trainer_ShoeLeftRed", vertices=v_shoe_l, faces=f_shoe_l))
    v_shoe_r, f_shoe_r = create_box((0.06, 0.0, -0.07), (0.26, 0.16, 0.25))
    groups.append(dict(name="Trainer_ShoeRightRed", vertices=v_shoe_r, faces=f_shoe_r))
    
    write_obj_file(filepath, groups)
    print(f"Generated Trainer model: {filepath}")

# --- Tree Model ---
def generate_tree(filepath):
    groups = []
    # Trunk
    v_trunk, f_trunk = create_cylinder(center=(0, 1.5, 0), radius_bottom=0.35, radius_top=0.25, height=3.0, steps=12)
    groups.append(dict(name="Tree_TrunkBrown", vertices=v_trunk, faces=f_trunk))
    # Foliage Layer 1 (Bottom)
    v_fol_1, f_fol_1 = create_cylinder(center=(0, 3.2, 0), radius_bottom=1.8, radius_top=0.2, height=2.2, steps=14)
    groups.append(dict(name="Tree_Foliage1Green", vertices=v_fol_1, faces=f_fol_1))
    # Foliage Layer 2 (Middle)
    v_fol_2, f_fol_2 = create_cylinder(center=(0, 4.3, 0), radius_bottom=1.4, radius_top=0.15, height=2.0, steps=14)
    groups.append(dict(name="Tree_Foliage2Green", vertices=v_fol_2, faces=f_fol_2))
    # Foliage Layer 3 (Top)
    v_fol_3, f_fol_3 = create_cylinder(center=(0, 5.2, 0), radius_bottom=1.0, radius_top=0.05, height=1.6, steps=14)
    groups.append(dict(name="Tree_Foliage3Green", vertices=v_fol_3, faces=f_fol_3))
    
    write_obj_file(filepath, groups)
    print(f"Generated Tree model: {filepath}")

# ==============================================================================
# 2. Audio Generation (WAV Format)
# ==============================================================================

def write_wav(filename, samples, sample_rate=44100):
    with wave.open(filename, 'w') as wf:
        wf.setnchannels(1)
        wf.setsampwidth(2) # 16-bit
        wf.setframerate(sample_rate)
        packed = struct.pack(f'<{len(samples)}h', *[max(-32768, min(32767, int(s * 32767))) for s in samples])
        wf.writeframes(packed)
    print(f"Generated Audio: {filename}")

def generate_throw_sound(filepath):
    # Fast whoosh wind throw sound (0.45s)
    sr = 44100
    duration = 0.45
    num_samples = int(sr * duration)
    samples = []
    for i in range(num_samples):
        t = i / sr
        env = math.sin(math.pi * (t / duration)) ** 1.5
        noise = (random.random() * 2 - 1) * 0.4
        freq = 300 + 500 * (t / duration)
        sine = math.sin(2 * math.pi * freq * t) * 0.6
        val = (noise + sine) * env
        samples.append(val)
    write_wav(filepath, samples, sr)

def generate_hit_sound(filepath):
    # Solid metallic / mechanical impact click (0.35s)
    sr = 44100
    duration = 0.35
    num_samples = int(sr * duration)
    samples = []
    for i in range(num_samples):
        t = i / sr
        env = math.exp(-18 * t)
        sine1 = math.sin(2 * math.pi * 520 * t) * 0.5
        sine2 = math.sin(2 * math.pi * 1040 * t) * 0.3
        noise = (random.random() * 2 - 1) * math.exp(-35 * t) * 0.5
        val = (sine1 + sine2 + noise) * env
        samples.append(val)
    write_wav(filepath, samples, sr)

def generate_capture_sound(filepath):
    # Energy swirl / absorption ray rising sound (1.2s)
    sr = 44100
    duration = 1.2
    num_samples = int(sr * duration)
    samples = []
    for i in range(num_samples):
        t = i / sr
        env = math.sin(math.pi * (t / duration)) ** 0.8
        freq = 350 + 900 * (t / duration) ** 1.5
        tremolo = 1.0 + 0.3 * math.sin(2 * math.pi * 18 * t)
        val = math.sin(2 * math.pi * freq * t) * tremolo * 0.5 * env
        # add sparkle harmonics
        sparkle = math.sin(2 * math.pi * (freq * 2.5) * t) * 0.2 * env
        samples.append(val + sparkle)
    write_wav(filepath, samples, sr)

def generate_success_sound(filepath):
    # Pokeball shakes (3 clicks) followed by triumph chime fanfare (2.0s)
    sr = 44100
    duration = 2.0
    num_samples = int(sr * duration)
    samples = [0.0] * num_samples
    
    # 3 shakes at 0.2s, 0.6s, 1.0s
    shakes = [0.2, 0.6, 1.0]
    for st in shakes:
        s_idx = int(st * sr)
        for i in range(int(0.12 * sr)):
            idx = s_idx + i
            if idx < num_samples:
                t = i / sr
                env = math.exp(-30 * t)
                val = (math.sin(2 * math.pi * 600 * t) + (random.random() * 2 - 1) * 0.3) * env * 0.4
                samples[idx] += val
                
    # Triumph fanfare notes: C5 (523), E5 (659), G5 (784), C6 (1046) starting at 1.25s
    notes = [(1.25, 523.25, 0.15), (1.40, 659.25, 0.15), (1.55, 783.99, 0.18), (1.70, 1046.50, 0.30)]
    for start_t, freq, note_len in notes:
        start_idx = int(start_t * sr)
        n_samples = int(note_len * sr)
        for i in range(n_samples):
            idx = start_idx + i
            if idx < num_samples:
                t = i / sr
                env = math.sin(math.pi * (i / n_samples)) ** 0.5
                harmonic = math.sin(2 * math.pi * freq * t) * 0.5 + math.sin(2 * math.pi * freq * 2 * t) * 0.25
                samples[idx] += harmonic * env * 0.5
                
    write_wav(filepath, samples, sr)

# ==============================================================================
# 3. Terrain Textures Generation
# ==============================================================================

def generate_textures():
    # Grass texture: Lush natural green with subtle tonal shifts
    size = (512, 512)
    grass_img = Image.new("RGB", size, (72, 138, 48))
    draw_g = ImageDraw.Draw(grass_img)
    for _ in range(8000):
        x = random.randint(0, 511)
        y = random.randint(0, 511)
        shade = random.randint(-25, 25)
        r = max(0, min(255, 72 + shade))
        g = max(0, min(255, 142 + shade * 2))
        b = max(0, min(255, 48 + shade))
        draw_g.line([(x, y), (x + random.randint(-2, 2), y + random.randint(-4, 4))], fill=(r, g, b), width=1)
    grass_path = os.path.join(textures_dir, "GrassTexture.png")
    grass_img.save(grass_path)
    print(f"Generated Texture: {grass_path}")
    
    # Rock texture: Rugged mountain granite grey/brown
    rock_img = Image.new("RGB", size, (120, 115, 110))
    draw_r = ImageDraw.Draw(rock_img)
    for _ in range(12000):
        x = random.randint(0, 511)
        y = random.randint(0, 511)
        shade = random.randint(-40, 40)
        r = max(0, min(255, 120 + shade))
        g = max(0, min(255, 115 + shade))
        b = max(0, min(255, 110 + shade))
        draw_r.rectangle([x, y, x + random.randint(1, 4), y + random.randint(1, 4)], fill=(r, g, b))
    rock_path = os.path.join(textures_dir, "RockTexture.png")
    rock_img.save(rock_path)
    print(f"Generated Texture: {rock_path}")

# Run all generators
if __name__ == "__main__":
    generate_pokeball(os.path.join(models_dir, "Pokeball.obj"))
    generate_pokemon(os.path.join(models_dir, "Pokemon.obj"))
    generate_trainer(os.path.join(models_dir, "Trainer.obj"))
    generate_tree(os.path.join(models_dir, "Tree.obj"))
    
    generate_throw_sound(os.path.join(audio_dir, "Pokeball_Throw.wav"))
    generate_hit_sound(os.path.join(audio_dir, "Pokeball_Hit.wav"))
    generate_capture_sound(os.path.join(audio_dir, "Pokeball_Capture.wav"))
    generate_success_sound(os.path.join(audio_dir, "Pokeball_Success.wav"))
    
    generate_textures()
    print("All HW5 Assets successfully generated!")
