public static class Shakepedia {
    public static CameraShakeProfile MINOR = new(0.08F, 0.8F, 5F, 1F, 0.1F, 1F, 0.003F, 3F);

    public static CameraShakeProfile MILD = new(0.08F, 10F, 5F, 1F, 0.6F, 1F, 0.003F, 3F);

    public static CameraShakeProfile MEDIUM_RARE = new(0.1F, 0.2F, 10F, 1F, 0.6F, 1F, 0.003F, 6F);
    public static CameraShakeProfile POW = new(0.1F, 0.6F, 10F, 10F, 0.6F, 1F, 0.003F, 11F);

    public static CameraShakeProfile RUMBLE = new(0.5F, 0.5F, 0.5F, 0.5F, 0.5F, 0.5F, 1F, 0.05F);

    public static CameraShakeProfile GetProfileClone(CameraShakeProfile profile) {
        return new CameraShakeProfile(
            profile.jitterRandXY,
            profile.jitterRandZ,
            profile.matrixRand,
            profile.angleMultiplier,
            profile.curveScale,
            profile.loopCount,
            profile.decay,
            profile.initialIntensity);
    }
}