#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 finalColor;

uniform sampler2D texture0;
uniform float time;
uniform vec2 resolution;

#define STAR_COUNT 700.0

float hash12(vec2 p) {
	vec3 p3  = fract(vec3(p.xyx) * .1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}

void main() {
    // 1. Base Layer: The Background Image
    vec4 texColor = texture(texture0, fragTexCoord);
    
    // 2. UV Setup for flying particles
    vec2 uv = (gl_FragCoord.xy - 0.5 * resolution.xy) / min(resolution.y, resolution.x);
    vec3 particlesCol = vec3(0.0);
    
    for (float i = 0.0; i < STAR_COUNT; i++) {
        // Randomization seeds
        float seed = i / STAR_COUNT;
        float angle = hash12(vec2(i, 456.789)) * 6.28318;
        float dist_seed = hash12(vec2(i, 123.456));
        float speed = 0.15 + 0.2 * hash12(vec2(i, 789.123));
        
        // Time cycle for the particle (flying from center to edge)
        float t = fract(time * speed + dist_seed);
        
        // Accelerating radial position
        float r = t * 1.5; 
        vec2 starPos = vec2(cos(angle), sin(angle)) * r;
        
        // Size grows as it approaches edge
        float size = 0.001 + 0.015 * t * t;
        float d = length(uv - starPos);
        
        if (d < size * 5.0) {
            // Particle is visible, let's determine its color
            // Transform particle radial position BACK to a texture coordinate
            // to sample a "source" color from the original nebula/galaxy image.
            // We shift the sampling slightly based on seed to get diverse colors.
            vec2 sampleUV = vec2(0.5) + vec2(cos(angle), sin(angle)) * (dist_seed * 0.4);
            vec4 sampledColor = texture(texture0, sampleUV);
            
            // Heuristic: If we sampled a dark/black pixel, default to white star
            // otherwise use the vibrant nebula color.
            float luminance = dot(sampledColor.rgb, vec3(0.299, 0.587, 0.114));
            vec3 pCol = (luminance > 0.1) ? sampledColor.rgb : vec3(1.0);
            
            // Rendering the particle glow
            float brightness = smoothstep(size, size * 0.5, d);
            brightness += 0.4 * smoothstep(size * 4.0, 0.0, d);
            
            // Visibility factors
            float alpha = smoothstep(0.0, 0.1, t) * smoothstep(1.0, 0.8, t);
            
            // Add the colored glow
            particlesCol += pCol * brightness * alpha;
        }
    }
    
    // Combine: Background + Flying Galaxy Particles
    // Note: We only add particles where the original background has some presence (alpha)
    finalColor = vec4(texColor.rgb + (particlesCol * texColor.a), texColor.a);
}
