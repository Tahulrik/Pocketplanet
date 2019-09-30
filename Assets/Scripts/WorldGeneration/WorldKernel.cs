using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WorldKernel
{
    public static float[,] ApplySquareKernel(float[,] inputMap, int x, int y, int kernelSize)
    {
        float[,] outputMap = (float[,])inputMap.Clone();
        int mapX = outputMap.GetLength(0);
        int mapY = outputMap.GetLength(1);

        int kernelCenterX = x;
        int kernelCenterY = y;
        float resultval = inputMap[kernelCenterX, kernelCenterY] < 0.5f ? 0f : 1f;
        for (int kernelY = -kernelSize; kernelY <= kernelSize * 2; kernelY++)
        {
            int kernelPosY = kernelCenterY + kernelY;
            if (kernelPosY < 0 || kernelPosY >= mapY)
                continue;

            for (int kernelX = -kernelSize; kernelX <= kernelSize * 2; kernelX++)
            {
                int kernelPosX = kernelCenterX + kernelX;

                if (kernelPosX < 0 || kernelPosX >= mapX)
                    continue;

                outputMap[kernelPosX, kernelPosY] = resultval;
            }
        }

        return outputMap;
    }
}
