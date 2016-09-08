using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuadTreeTest : MonoBehaviour {
    private QuadTree<int> tree;
    // Use this for initialization
    private GameObject cube;
    void Start() {

        tree = new QuadTree<int>(new Bound2D(new Vector2(0, 0), new Vector2(100, 100)), 1);

        for (int i = 0; i < 100; ++i)
        {
            int x = Random.Range(0, 99);
            int y = Random.Range(0, 99);

            tree.Insert(x * y, new Bound2D(new Vector2(x, y),
                                            new Vector2(x + 0.1f, y + 0.1f)));

        }
        cube = GameObject.Find("Cube");


        //List<int> elements = new List<int>();
        //var box = new Bound2D(new Vector2(0, 0), new Vector2(100, 10));
        //tree.GetElements(box, elements);

        //foreach(var val in elements)
        //{
        //    Debug.Log(val);
        //}
        //Debug.Log(elements.Count);
        Debug.Log("finish");
    }


    // Update is called once per frame
    void Update() {
      
    }

    void OnPostRender()
    {
        QuadTree<int>.DrawTree(tree,cube.transform.localToWorldMatrix);
    }
}
