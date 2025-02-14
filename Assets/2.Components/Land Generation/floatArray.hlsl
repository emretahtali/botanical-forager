StructuredBuffer<float3> Positions;

void floatArray_float(out float3 Out)
{
    Out = Positions[0];
}
