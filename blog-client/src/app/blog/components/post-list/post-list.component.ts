import { Component, OnInit } from '@angular/core';
import { PostParameters } from '../../Model/Post-Parameters';
import { PageMeta } from 'src/app/shared/Model/page-meta';
import { ResultWithLinks } from 'src/app/shared/Model/result-with-link';
import { Post } from '../../Model/post';
import { OpenIdConnectService } from 'src/app/shared/oidc/open-id-connect.service';
import { PostService } from '../../service/post.service';

@Component({
  selector: 'app-post-list',
  templateUrl: './post-list.component.html',
  styleUrls: ['./post-list.component.scss']
})
export class PostListComponent implements OnInit {
  constructor(
    private postService: PostService,
    private openIdConnectService: OpenIdConnectService
  ) {}
  posts: Post[];
  pageMeta: PageMeta;
  postparameter = new PostParameters({
    orderBy: 'id desc',
    pageIndex: 0,
    pageSize: 3
  });

  ngOnInit() {
    this.posts = [];
    this.getposts();
  }

  getposts() {
    this.postService.getPagedPosts(this.postparameter).subscribe(resp => {
      this.pageMeta = JSON.parse(resp.headers.get('x-Pagination')) as PageMeta;
      const result = { ...resp.body } as ResultWithLinks<Post>;
      this.posts = this.posts.concat(result.value);
    });
  }

  onScroll() {
    console.log('scrolled down!!');
    this.postparameter.pageIndex++;
    if (this.postparameter.pageIndex < this.pageMeta.pageCount) {
      this.getposts();
    }
  }
}
