using System;
using System.Collections.Generic;
using UnityEngine;

public class QuadTree<T>
{
    class QuadTreeNode
    {
        public Bound2D bound;
        public T element;

        public QuadTreeNode(T v, Bound2D box)
        {
            bound = box;
            element = v;
        }
    }

    //enum QuadNames
    //{
    static private readonly int TopLeft = 0;
    static private readonly int TopRight = 1;
    static private readonly int BottomLeft = 2;
    static private readonly int BottomRight = 3;
    //}


    private List<QuadTreeNode> nodeList = new List<QuadTreeNode>();

    static private readonly int NodeCapacity = 4;

    private QuadTree<T>[] subTrees;

    public Bound2D treeBox;

    public Vector2 treeCenter;

    private float minimumQuadSize;

    /** Whether this is a leaf or an internal sub-tree */
    private bool bInternal;

    public QuadTree(Bound2D box, float minSize)
    {
        treeBox = box;
        treeCenter = box.center;
        minimumQuadSize = minSize;
        subTrees = new QuadTree<T>[NodeCapacity];
    }


    public void Insert(T element, Bound2D box)
    {
        if (!box.Intersect(treeBox))
        {
            //todo log
        }
        InsertElementRecursive(element, box);
    }


    public void GetElements(Bound2D box, List<T> elementsOut)
    {
        QuadTree<T>[] Quads;
        int NumQuads = GetQuads(box, out Quads);

        // Always include any nodes contained in this quad
        GetIntersectingElements(box, elementsOut);

        // As well as all relevant subtrees
        for (int QuadIndex = 0; QuadIndex < NumQuads; QuadIndex++)
        {
            Quads[QuadIndex].GetElements(box, elementsOut);
        }
    }

    public bool Remove(T element, Bound2D box)
    {
        bool bElementRemoved = false;

        QuadTree<T>[] Quads;
        int NumQuads = GetQuads(box, out Quads);

        // Remove from nodes referenced by this quad
        bElementRemoved = RemoveNodeForElement(element);

        // Try to remove from subtrees if necessary
        for (int QuadIndex = 0; QuadIndex < NumQuads && !bElementRemoved; QuadIndex++)
        {
            bElementRemoved = Quads[QuadIndex].Remove(element, box);
        }

        return bElementRemoved;
    }


    private bool RemoveNodeForElement(T element)
    {
        int ElementIdx = -1;
        for (int NodeIdx = 0, NumNodes = nodeList.Count; NodeIdx < NumNodes; ++NodeIdx)
        {
            if (nodeList[NodeIdx].element.Equals(element))
            {
                ElementIdx = NodeIdx;
                break;
            }
        }

        if (ElementIdx != -1)
        {
            nodeList.RemoveAt(ElementIdx);
            return true;
        }

        return false;
    }

    private void GetIntersectingElements(Bound2D box, List<T> elementsOut)
    {
        foreach (var node in nodeList)
        {
            if (box.Intersect(node.bound))
            {
                elementsOut.Add(node.element);
            }
        }
    }

    private void InsertElementRecursive(T element, Bound2D box)
    {
        QuadTree<T>[] Quads;
        int NumQuads = GetQuads(box, out Quads);
        if (NumQuads == 0)
        {
            // This should only happen for leaves
            //check(!bInternal);

            // It's possible that all elements in the leaf are bigger than the leaf or that more elements than NodeCapacity exist outside the top level quad
            // In either case, we can get into an endless spiral of splitting
            bool bCanSplitTree = treeBox.GetSize().magnitude > minimumQuadSize;
            if (!bCanSplitTree || nodeList.Count < NodeCapacity)
            {
                nodeList.Add(new QuadTreeNode(element, box));

                if (!bCanSplitTree)
                {
                    //UE_LOG(LogQuadTree, Warning, TEXT("Minimum size %f reached for quadtree at %s. Filling beyond capacity %d to %d"), minimumQuadSize, *Position.ToString(), NodeCapacity, Nodes.Num());
                }
            }
            else
            {
                // This quad is at capacity, so split and try again
                Split();
                InsertElementRecursive(element, box);
            }
        }
        else if (NumQuads == 1)
        {
            //check(bInternal);

            // Fully contained in a single subtree, so insert it there
            Quads[0].InsertElementRecursive(element, box);
        }
        else
        {
            // Overlaps multiple subtrees, store here
            //check(bInternal);
            nodeList.Add(new QuadTreeNode(element, box));
        }
    }


    private void Split()
    {
        //check(bInternal == false);

        Vector2 Extent = treeBox.GetExtent();
        Vector2 XExtent = new Vector2(Extent.x, 0.0f);
        Vector2 YExtent = new Vector2(0.0f, Extent.y);

        /************************************************************************
         *  ___________max
         * |     |     |
         * |     |     |
         * |-----c------
         * |     |     |
         * min___|_____|
         *
         * We create new quads by adding xExtent and yExtent
         ************************************************************************/

        Vector2 C = treeCenter;
        Vector2 TM = C + YExtent;
        Vector2 ML = C - XExtent;
        Vector2 MR = C + XExtent;
        Vector2 BM = C - YExtent;
        Vector2 BL = treeBox.min;
        Vector2 TR = treeBox.max;

        subTrees[TopLeft] = new QuadTree<T>(new Bound2D(ML, TM), minimumQuadSize);
        subTrees[TopRight] = new QuadTree<T>(new Bound2D(C, TR), minimumQuadSize);
        subTrees[BottomLeft] = new QuadTree<T>(new Bound2D(BL, C), minimumQuadSize);
        subTrees[BottomRight] = new QuadTree<T>(new Bound2D(BM, MR), minimumQuadSize);

        //mark as no longer a leaf
        bInternal = true;

        // Place existing nodes and place them into the new subtrees that contain them
        // If a node overlaps multiple subtrees, we retain the reference to it here in this quad

        List<QuadTreeNode> OverlappingNodes = new List<QuadTreeNode>();
        foreach (var node in nodeList)
        {
            QuadTree<T>[] Quads;
            int NumQuads = GetQuads(node.bound, out Quads);
            if (NumQuads == 1)
            {
                Quads[0].nodeList.Add(node);
            }
            else
            {
                OverlappingNodes.Add(node);
            }
        }
        nodeList = OverlappingNodes;

    }

    private int GetQuads(Bound2D box, out QuadTree<T>[] quads)
    {
        List<QuadTree<T>> listTree = new List<QuadTree<T>>();
        if (bInternal)
        {
            bool bNegX = box.min.x <= treeCenter.x;
            bool bNegY = box.min.y <= treeCenter.y;

            bool bPosX = box.max.x >= treeCenter.x;
            bool bPosY = box.max.y >= treeCenter.y;

            if (bNegX && bNegY)
            {
                listTree.Add(subTrees[BottomLeft]);
            }

            if (bPosX && bNegY)
            {
                listTree.Add(subTrees[BottomRight]);
            }

            if (bNegX && bPosY)
            {
                listTree.Add(subTrees[TopLeft]);
            }

            if (bPosX && bPosY)
            {
                listTree.Add(subTrees[TopRight]);
            }
        }
        quads = listTree.ToArray();
        return listTree.Count;
    }


    static public void DrawTree(QuadTree<T> tree, Matrix4x4 mat)
    {
       // if (tree.bInternal)
        {
            DrawBounds(tree.treeBox, Color.black, mat);
            foreach (var node in tree.nodeList)
            {
                DrawBounds(node.bound, Color.red, mat);
            }
            foreach (var subtree in tree.subTrees)
            {
                if (subtree != null)
                    DrawTree(subtree, mat);
            }
        }
    }

    static public void DrawBounds(Bound2D box,Color c, Matrix4x4 mat)
    {

        Vector2 Extent = box.GetExtent();
        Vector2 XExtent = new Vector2(Extent.x, 0.0f);
        Vector2 YExtent = new Vector2(0.0f, Extent.y);
        Vector2 C = box.center;
        Vector2 TM = C + YExtent + XExtent;
        Vector2 ML = C + YExtent - XExtent;
        Vector2 MR = C - YExtent - XExtent;
        Vector2 BM = C - YExtent + XExtent;
   
        GL.PushMatrix();
        GL.MultMatrix(mat);
        GL.Begin(GL.LINES);
        GL.Color(c);
        GL.Vertex3(TM.x, 0, TM.y);
        GL.Vertex3(ML.x, 0, ML.y);
        GL.Vertex3(ML.x, 0, ML.y);
        GL.Vertex3(MR.x, 0, MR.y);
        GL.Vertex3(MR.x, 0, MR.y);
        GL.Vertex3(BM.x, 0, BM.y);
        GL.Vertex3(BM.x, 0, BM.y);
        GL.Vertex3(TM.x, 0, TM.y);
        GL.End();
        GL.PopMatrix();
    }

}


