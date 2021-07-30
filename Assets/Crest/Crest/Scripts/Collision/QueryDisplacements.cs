﻿// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

using System.Collections.Generic;
using UnityEngine;

namespace Crest
{
    /// <summary>
    /// Samples water surface shape - displacement, height, normal, velocity.
    /// </summary>
    public class QueryDisplacements : QueryBase, ICollProvider
    {
        readonly int sp_LD_TexArray_AnimatedWaves = Shader.PropertyToID("_LD_TexArray_AnimatedWaves");
        readonly int sp_ResultDisplacements = Shader.PropertyToID("_ResultDisplacements");

        protected override string QueryShaderName => "QueryDisplacements";
        protected override string QueryKernelName => "CSMain";

        protected override void BindInputsAndOutputs(PropertyWrapperComputeStandalone wrapper, ComputeBuffer resultsBuffer)
        {
            LodDataMgrAnimWaves.Bind(wrapper);
            ShaderProcessQueries.SetTexture(_kernelHandle, sp_LD_TexArray_AnimatedWaves, OceanRenderer.Instance._lodDataAnimWaves.DataTexture);
            ShaderProcessQueries.SetBuffer(_kernelHandle, sp_ResultDisplacements, resultsBuffer);

            ShaderProcessQueries.SetBuffer(_kernelHandle, OceanRenderer.sp_cascadeData, OceanRenderer.Instance._bufCascadeDataTgt);
        }

        public int Query(int i_ownerHash, float i_minSpatialLength, IList<Vector3> i_queryPoints,
            IList<float> o_resultHeights, IList<Vector3> o_resultNorms, IList<Vector3> o_resultVels)
        {
            var result = (int)QueryStatus.OK;

            if (!UpdateQueryPoints(i_ownerHash, i_minSpatialLength, (Vector3[]) i_queryPoints, (Vector3[]) (o_resultNorms != null ? i_queryPoints : null)))
            {
                result |= (int)QueryStatus.PostFailed;
            }

            if (!RetrieveResults(i_ownerHash, null, (float[]) o_resultHeights, (Vector3[]) o_resultNorms))
            {
                result |= (int)QueryStatus.RetrieveFailed;
            }

            if (o_resultVels != null)
            {
                result |= CalculateVelocities(i_ownerHash, (Vector3[]) o_resultVels);
            }

            return result;
        }
    }
}
