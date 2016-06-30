using UnityEngine;

public static class TransformExtension
{
    public static void SetParentAndScale(this Transform childTrans, Transform parentTrans, bool isActive = true)
    {
        childTrans.parent = parentTrans;
        childTrans.localScale = Vector3.one;
        childTrans.gameObject.SetActive(isActive);
    }
    public static Mesh Copy(this Mesh me)
    {

        Mesh mesh = new Mesh();
        if (me == null)
            return mesh;
        mesh.vertices = me.vertices;
        mesh.colors = me.colors;
        mesh.colors32 = me.colors32;
        mesh.uv = me.uv;
        mesh.uv2 = me.uv2;
        mesh.uv3 = me.uv3;
        mesh.uv4 = me.uv4;
        mesh.triangles = me.triangles;
        mesh.normals = me.normals;
        mesh.bindposes = me.bindposes;
        mesh.boneWeights = me.boneWeights;
        mesh.bounds = me.bounds;
        mesh.UploadMeshData(false);
        return mesh;
        //mesh. = meshFil.sharedMesh.bounds;
        //mesh.bounds = meshFil.sharedMesh.bounds;
    }

}
