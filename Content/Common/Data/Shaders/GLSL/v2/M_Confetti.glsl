#define URHO3D_PIXEL_NEED_TEXCOORD

#ifndef URHO3D_VERTEX_HAS_TEXCOORD0
#define URHO3D_VERTEX_HAS_TEXCOORD0
#endif

#ifndef URHO3D_VERTEX_HAS_COLOR
#define URHO3D_VERTEX_HAS_COLOR
#endif


#define URHO3D_MATERIAL_ALBEDO URHO3D_TEXTURE_ALBEDO
#define URHO3D_MATERIAL_NORMAL URHO3D_TEXTURE_NORMAL
#define URHO3D_MATERIAL_PROPERTIES URHO3D_TEXTURE_PROPERTIES
#define URHO3D_MATERIAL_EMISSION URHO3D_TEXTURE_EMISSION
#define URHO3D_CUSTOM_MATERIAL_UNIFORMS

#include "_Config.glsl"
#include "_Uniforms.glsl"
#include "_DefaultSamplers.glsl"

UNIFORM_BUFFER_BEGIN(4, Material)
    DEFAULT_MATERIAL_UNIFORMS
    UNIFORM(half cAnimationPhase)
UNIFORM_BUFFER_END(4, Material)

#include "_Material.glsl"

#ifdef URHO3D_VERTEX_SHADER

mat3 FromAngleAxis(float angle, vec3 normAxis)
{
    float sinAngle = sin(angle);
    float cosAngle = cos(angle);
    float _cosAngle = 1.0 - cosAngle;

    return mat3(
    cosAngle + normAxis.x * normAxis.x * _cosAngle,
    normAxis.y * normAxis.x * _cosAngle + normAxis.z * sinAngle,
    normAxis.z * normAxis.x * _cosAngle - normAxis.y * sinAngle,

    normAxis.x * normAxis.y * _cosAngle - normAxis.z * sinAngle,
    cosAngle + normAxis.y * normAxis.y * _cosAngle,
    normAxis.z * normAxis.y * _cosAngle + normAxis.x * sinAngle,

    normAxis.x * normAxis.z * _cosAngle + normAxis.y * sinAngle,
    normAxis.y * normAxis.z * _cosAngle - normAxis.x * sinAngle,
    cosAngle + normAxis.z * normAxis.z * _cosAngle);
}

void main()
{
    mat4 modelMatrix = GetModelMatrix();

    float t = cAnimationPhase;
    float _t = 1.0 - t;
    vec3 bernsteinCoefficients = vec3(_t*_t, 2.0*t*_t, t*t);

    vec3 offset = FromAngleAxis(iColor.w + t * 10.0, iNormal) * vec3(iTexCoord.x-0.5, 0.0, -iTexCoord.y+0.5);

    float up = dot(bernsteinCoefficients, vec3(0.0, 0.0, -0.5));
    float size = dot(bernsteinCoefficients, vec3(0.0, 0.1, 0.0));
    float dist = dot(bernsteinCoefficients, vec3(0.0, 1.0, 1.0));

    VertexTransform vertexTransform;
    vertexTransform.position = vec4(iPos.xyz*dist + offset.xyz*size + vec3(0.0, up, 0.0), 1.0) * modelMatrix;

    #ifdef URHO3D_VERTEX_NEED_NORMAL
        mediump mat3 normalMatrix = GetNormalMatrix(modelMatrix);
        vertexTransform.normal = normalize(iNormal * normalMatrix);

        ApplyShadowNormalOffset(result.position, result.normal);

        #ifdef URHO3D_VERTEX_NEED_TANGENT
            vertexTransform.tangent = normalize(iTangent.xyz * normalMatrix);
            vertexTransform.bitangent = cross(result.tangent, result.normal) * iTangent.w;
        #endif
    #endif

    Vertex_SetAll(vertexTransform, cNormalScale, cUOffset, cVOffset, cLMOffset);
}
#endif

#ifdef URHO3D_PIXEL_SHADER
void main()
{
#ifdef URHO3D_DEPTH_ONLY_PASS
    Pixel_DepthOnly(sAlbedo, vTexCoord);
#else
    SurfaceData surfaceData;

    Surface_SetCommon(surfaceData);
    Surface_SetAmbient(surfaceData, sEmission, vTexCoord2);
    Surface_SetNormal(surfaceData, vNormal, sNormal, vTexCoord, vTangent, vBitangentXY);
    Surface_SetPhysicalProperties(surfaceData, cRoughness, cMetallic, cDielectricReflectance, sProperties, vTexCoord);
    Surface_SetLegacyProperties(surfaceData, cMatSpecColor.a, sEmission, vTexCoord);
    Surface_SetCubeReflection(surfaceData, sReflection0, sReflection1, vReflectionVec, vWorldPos);
    Surface_SetPlanarReflection(surfaceData, sReflection0, cReflectionPlaneX, cReflectionPlaneY);
    Surface_SetBackground(surfaceData, sEmission, sDepthBuffer);
    Surface_SetBaseAlbedo(surfaceData, cMatDiffColor, cAlphaCutoff, vColor, sAlbedo, vTexCoord, URHO3D_MATERIAL_ALBEDO);
    Surface_SetBaseSpecular(surfaceData, cMatSpecColor, cMatEnvMapColor, sProperties, vTexCoord);
    Surface_SetAlbedoSpecular(surfaceData);
    Surface_SetEmission(surfaceData, cMatEmissiveColor, sEmission, vTexCoord, URHO3D_MATERIAL_EMISSION);
    Surface_ApplySoftFadeOut(surfaceData, vWorldDepth, cFadeOffsetScale);

    half3 surfaceColor = GetSurfaceColor(surfaceData);
    gl_FragColor = GetFragmentColorAlpha(surfaceColor, surfaceData.albedo.a, surfaceData.fogFactor);
#endif
}
#endif
